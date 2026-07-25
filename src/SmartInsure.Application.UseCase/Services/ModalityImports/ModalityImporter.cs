using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.Services.ModalityImports;

/// <summary>
/// Importação de modalidades (RN-034): por Corretora, chama o Motor de Cálculo resolvido pela
/// Habilitação, casa as Seguradoras retornadas por Referência de origem, faz upsert do lado
/// importado (RN-033), deriva a Modalidade da Modalidade Global por *find-or-create* pelo id
/// global e vincula a Importada (RN-035), preservando override Manual (RN-037); desativa o que
/// sumiu numa importação bem-sucedida e isola a falha por Corretora/Seguradora (RN-038) (ADR-061).
/// Após o upsert, importa a Tag (RN-047) e as Cláusulas particulares (RN-048) de cada Modalidade
/// Importada Ativa pelo objeto da modalidade, com resiliência e reconciliação (RN-049).
/// </summary>
public sealed class ModalityImporter(
    IBrokerageInsurerEnablementRepository enablementRepository,
    IImportedGroupRepository importedGroupRepository,
    IImportedModalityRepository importedModalityRepository,
    IModalityRepository modalityRepository,
    IImportedModalityTagRepository tagRepository,
    IImportedModalityParticularClauseRepository clauseRepository,
    IServiceProvider serviceProvider,
    IUnitOfWork unitOfWork) : IModalityImporter
{
    public async Task<ModalityImportSummary> RunAsync(DateTime nowUtc, CancellationToken cancellationToken)
    {
        var enablements = await enablementRepository.ListActiveForImportAsync(cancellationToken);

        var processedInsurers = new HashSet<Guid>();
        var failures = new List<string>();
        int processed = 0, succeeded = 0, failed = 0;

        // Cache de Modalidades por id global, ao longo de toda a execução: uma Modalidade Global é
        // compartilhada entre Seguradoras e o find-or-create por consulta não enxerga o que ainda
        // não foi commitado — sem o cache criaria Modalidades duplicadas para o mesmo id global.
        var modalityCache = new Dictionary<string, Modality>();

        foreach (var broker in enablements.GroupBy(enablement => enablement.BrokerageId))
        {
            var brokerEnablements = broker.ToList();
            var brokerCnpj = brokerEnablements[0].BrokerCnpj;

            // OPEN-09: quando a Corretora tem várias Habilitações, usa-se a primeira Ativa para a chamada.
            var connection = brokerEnablements[0];

            var insurerByReference = brokerEnablements
                .Where(enablement => !string.IsNullOrWhiteSpace(enablement.InsurerReferenceExternalId))
                .GroupBy(enablement => enablement.InsurerReferenceExternalId!)
                .ToDictionary(group => group.Key, group => group.First().InsurerId);

            ImportedCatalogResult catalog;
            ICalculationEngine engine;

            try
            {
                engine = ResolveEngine(connection.CalculationEngine);
                catalog = await engine.GetGroupAndModalitiesAsync(
                    connection.ConnectionParameters, brokerCnpj, cancellationToken);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                // RN-038: falha da Corretora não desativa nada e não afeta as demais.
                foreach (var _ in insurerByReference)
                {
                    processed++;
                    failed++;
                }

                failures.Add($"Corretora {brokerCnpj}: {exception.Message}");
                continue;
            }

            foreach (var insurerCatalog in catalog.Insurers)
            {
                if (!insurerByReference.TryGetValue(insurerCatalog.InsuranceReferenceExternalId, out var insurerId)
                    || !processedInsurers.Add(insurerId))
                {
                    // Seguradora sem Habilitação Ativa conhecida, ou já processada por outra Corretora (dedup).
                    continue;
                }

                processed++;

                if (!insurerCatalog.IsSuccess)
                {
                    // RN-038: falha da Seguradora não desativa nada dela.
                    failed++;
                    failures.Add($"Seguradora {insurerCatalog.InsuranceName} ({brokerCnpj}): falha na origem (IsSuccess=false).");
                    continue;
                }

                try
                {
                    await ImportInsurerAsync(
                        insurerId, insurerCatalog, modalityCache, engine,
                        connection.ConnectionParameters, brokerCnpj, failures, nowUtc, cancellationToken);
                    await unitOfWork.CommitAsync(cancellationToken);
                    succeeded++;
                }
                catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
                {
                    failed++;
                    failures.Add($"Seguradora {insurerCatalog.InsuranceName}: {exception.Message}");
                }
            }
        }

        return new ModalityImportSummary(processed, succeeded, failed, failures);
    }

    private async Task ImportInsurerAsync(
        Guid insurerId,
        ImportedInsurerCatalog insurerCatalog,
        Dictionary<string, Modality> modalityCache,
        ICalculationEngine engine,
        string? connectionParameters,
        string brokerCnpj,
        List<string> failures,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var seenSourceIds = new HashSet<string>();
        // Cache in-batch: várias modalidades compartilham o mesmo Grupo Importado e o upsert por
        // consulta não enxerga o que ainda não foi commitado — sem o cache criaria grupos duplicados.
        var groupCache = new Dictionary<string, ImportedGroup>();
        var importedThisRun = new List<ImportedModality>();

        foreach (var data in insurerCatalog.Modalities)
        {
            // Duplicata do mesmo identificador de origem no retorno: a primeira ocorrência vence.
            if (!seenSourceIds.Add(data.SourceId))
            {
                continue;
            }

            var importedGroupId = await UpsertGroupAsync(insurerId, data, groupCache, cancellationToken);
            var imported = await UpsertModalityAsync(insurerId, data, importedGroupId, nowUtc, cancellationToken);
            await LinkToModalityAsync(imported, data, modalityCache, cancellationToken);
            importedThisRun.Add(imported);
        }

        // RN-038: Modalidades Importadas Ativas que não vieram nesta importação bem-sucedida são desativadas.
        var active = await importedModalityRepository.ListActiveByInsurerAsync(insurerId, cancellationToken);
        var deactivated = active.Where(modality => !seenSourceIds.Contains(modality.SourceId)).ToList();

        foreach (var modality in deactivated)
        {
            modality.Deactivate();
        }

        // RN-047/048/049: objeto da modalidade (Tag + Cláusulas) por Modalidade Importada Ativa desta execução.
        foreach (var imported in importedThisRun)
        {
            await ImportModalityObjectAsync(
                imported, engine, connectionParameters, brokerCnpj, insurerCatalog.InsuranceName, failures, cancellationToken);
        }

        // RN-049: Modalidade desativada pela reconciliação → inativa sua Tag e Cláusulas.
        foreach (var imported in deactivated)
        {
            await DeactivateModalityObjectAsync(imported, cancellationToken);
        }
    }

    private async Task<Guid> UpsertGroupAsync(
        Guid insurerId,
        ImportedModalityData data,
        Dictionary<string, ImportedGroup> groupCache,
        CancellationToken cancellationToken)
    {
        if (groupCache.TryGetValue(data.GroupSourceId, out var cached))
        {
            return cached.Id;
        }

        var group = await importedGroupRepository.GetByInsurerAndSourceAsync(
            insurerId, data.GroupSourceId, cancellationToken);

        if (group is null)
        {
            group = ImportedGroup.Create(insurerId, data.GroupSourceId, data.GroupName, data.GroupType);
            await importedGroupRepository.AddAsync(group, cancellationToken);
        }
        else
        {
            group.UpdateFromSource(data.GroupName, data.GroupType);
        }

        groupCache[data.GroupSourceId] = group;
        return group.Id;
    }

    private async Task<ImportedModality> UpsertModalityAsync(
        Guid insurerId, ImportedModalityData data, Guid importedGroupId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var imported = await importedModalityRepository.GetByInsurerAndSourceAsync(
            insurerId, data.SourceId, cancellationToken);

        if (imported is null)
        {
            imported = ImportedModality.Create(
                insurerId, data.SourceId, data.OriginName, data.Branch, data.EngineModalityId,
                data.EngineModalityName, importedGroupId, data.RawParameters, nowUtc);
            await importedModalityRepository.AddAsync(imported, cancellationToken);
        }
        else
        {
            imported.UpdateFromSource(
                data.OriginName, data.Branch, data.EngineModalityId, data.EngineModalityName,
                importedGroupId, data.RawParameters, nowUtc);
        }

        return imported;
    }

    /// <summary>
    /// RN-035: deriva a Modalidade da Modalidade Global (find-or-create pelo id global, nome da fonte)
    /// e vincula a Importada como Automatic — o override Manual é preservado (RN-037). Importada sem
    /// id de Modalidade Global (exceção) fica sem vínculo e vai à Fila (RN-037).
    /// </summary>
    private async Task LinkToModalityAsync(
        ImportedModality imported,
        ImportedModalityData data,
        Dictionary<string, Modality> modalityCache,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(data.EngineModalityId))
        {
            return;
        }

        var globalName = string.IsNullOrWhiteSpace(data.EngineModalityName)
            ? data.OriginName
            : data.EngineModalityName;

        var modality = await FindOrCreateModalityAsync(
            data.EngineModalityId, globalName, modalityCache, cancellationToken);

        imported.LinkToModality(modality.Id, EModalityLinkSource.Automatic);
    }

    private async Task<Modality> FindOrCreateModalityAsync(
        string globalModalityExternalId,
        string globalName,
        Dictionary<string, Modality> modalityCache,
        CancellationToken cancellationToken)
    {
        if (modalityCache.TryGetValue(globalModalityExternalId, out var cached))
        {
            return cached;
        }

        var modality = await modalityRepository.GetByGlobalExternalIdAsync(globalModalityExternalId, cancellationToken)
            ?? await CreateModalityAsync(globalModalityExternalId, globalName, cancellationToken);

        modalityCache[globalModalityExternalId] = modality;
        return modality;
    }

    private async Task<Modality> CreateModalityAsync(
        string globalModalityExternalId, string globalName, CancellationToken cancellationToken)
    {
        var modality = Modality.CreateFromGlobal(globalModalityExternalId, globalName);
        await modalityRepository.AddAsync(modality, cancellationToken);
        return modality;
    }

    private ICalculationEngine ResolveEngine(string calculationEngine)
    {
        var engine = Enum.Parse<ECalculationEngine>(calculationEngine);
        return serviceProvider.GetRequiredKeyedService<ICalculationEngine>(engine);
    }

    /// <summary>
    /// RN-047/048/049: importa o objeto (Tag + Cláusulas) de uma Modalidade Importada. Falha na consulta
    /// é isolada (não desativa nada, RN-049). Tag só é gravada com JsonTag preenchido (RN-047); objeto
    /// bem-sucedido sem JsonTag inativa a Tag existente. Cláusulas por chave externa (RN-048); ausentes
    /// numa consulta bem-sucedida são inativadas (RN-049).
    /// </summary>
    private async Task ImportModalityObjectAsync(
        ImportedModality imported,
        ICalculationEngine engine,
        string? connectionParameters,
        string brokerCnpj,
        string insurerName,
        List<string> failures,
        CancellationToken cancellationToken)
    {
        ModalityObjectResult result;

        try
        {
            result = await engine.GetModalityObjectAsync(
                connectionParameters, brokerCnpj, imported.SourceId, cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            // RN-049: falha da modalidade não desativa nada dela nem afeta as demais.
            failures.Add($"Objeto da modalidade {imported.SourceId} (Seguradora {insurerName}): {exception.Message}");
            return;
        }

        // Motor real nunca retorna nulo; defensivo para não desativar por engano (RN-049).
        if (result is null || result.HasError)
        {
            failures.Add($"Objeto da modalidade {imported.SourceId} (Seguradora {insurerName}): resposta com erro.");
            return;
        }

        await UpsertTagAsync(imported.Id, result, cancellationToken);
        await UpsertClausesAsync(imported.Id, result, cancellationToken);
    }

    private async Task UpsertTagAsync(Guid importedModalityId, ModalityObjectResult result, CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByImportedModalityAsync(importedModalityId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(result.JsonTag))
        {
            if (tag is null)
            {
                await tagRepository.AddAsync(
                    ImportedModalityTag.Create(importedModalityId, result.JsonTag, result.ObjectText), cancellationToken);
            }
            else
            {
                tag.UpdateFromSource(result.JsonTag, result.ObjectText);
            }
        }
        else if (tag is not null)
        {
            // RN-047: objeto bem-sucedido sem JsonTag não sobrescreve com vazio; RN-049: inativa a Tag existente.
            tag.Deactivate();
        }
    }

    private async Task UpsertClausesAsync(Guid importedModalityId, ModalityObjectResult result, CancellationToken cancellationToken)
    {
        var localClauses = await clauseRepository.ListByImportedModalityAsync(importedModalityId, cancellationToken);
        var seenExternalIds = new HashSet<string>();

        foreach (var clause in result.Clauses)
        {
            if (!seenExternalIds.Add(clause.ExternalId))
            {
                continue; // duplicata do mesmo id na resposta: a primeira vence (RN-048).
            }

            var existing = localClauses.FirstOrDefault(c => c.ExternalId == clause.ExternalId);

            if (existing is null)
            {
                await clauseRepository.AddAsync(
                    ImportedModalityParticularClause.Create(
                        importedModalityId, clause.ExternalId, clause.Name, clause.Text, clause.JsonTag),
                    cancellationToken);
            }
            else
            {
                existing.UpdateFromSource(clause.Name, clause.Text, clause.JsonTag);
            }
        }

        // RN-049: Cláusulas locais Ativas que não vieram na resposta bem-sucedida são inativadas.
        foreach (var local in localClauses.Where(c =>
                     c.Status == EImportedModalityClauseStatus.Active && !seenExternalIds.Contains(c.ExternalId)))
        {
            local.Deactivate();
        }
    }

    /// <summary>RN-049: Modalidade que saiu de Ativa (reconciliação de catálogo) tem Tag e Cláusulas inativadas.</summary>
    private async Task DeactivateModalityObjectAsync(ImportedModality imported, CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByImportedModalityAsync(imported.Id, cancellationToken);
        tag?.Deactivate();

        var clauses = await clauseRepository.ListByImportedModalityAsync(imported.Id, cancellationToken);

        foreach (var clause in clauses.Where(c => c.Status == EImportedModalityClauseStatus.Active))
        {
            clause.Deactivate();
        }
    }
}
