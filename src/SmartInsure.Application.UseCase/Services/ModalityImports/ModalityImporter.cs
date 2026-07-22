using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.Services.ModalityImports;

/// <summary>
/// Importação de modalidades (RN-031): por Corretora, chama o Motor de Cálculo resolvido pela
/// Habilitação, casa as Seguradoras retornadas por Referência de origem, faz upsert do lado
/// importado (RN-030), mapeia por identificador do motor dentro do mesmo ramo (RN-032), desativa
/// o que sumiu numa importação bem-sucedida e isola a falha por Corretora/Seguradora (RN-035).
/// Nunca cria Modalidade (lado Smart) — só o lado importado e mapeamentos por identificador (ADR-060).
/// </summary>
public sealed class ModalityImporter(
    IBrokerageInsurerEnablementRepository enablementRepository,
    IImportedGroupRepository importedGroupRepository,
    IImportedModalityRepository importedModalityRepository,
    IModalityMappingRepository modalityMappingRepository,
    IServiceProvider serviceProvider,
    IUnitOfWork unitOfWork) : IModalityImporter
{
    public async Task<ModalityImportSummary> RunAsync(DateTime nowUtc, CancellationToken cancellationToken)
    {
        var enablements = await enablementRepository.ListActiveForImportAsync(cancellationToken);

        var processedInsurers = new HashSet<Guid>();
        var failures = new List<string>();
        int processed = 0, succeeded = 0, failed = 0;

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

            try
            {
                var engine = ResolveEngine(connection.CalculationEngine);
                catalog = await engine.GetGroupAndModalitiesAsync(
                    connection.ConnectionParameters, brokerCnpj, cancellationToken);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                // RN-035: falha da Corretora não desativa nada e não afeta as demais.
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
                    // RN-035: falha da Seguradora não desativa nada dela.
                    failed++;
                    failures.Add($"Seguradora {insurerCatalog.InsuranceName} ({brokerCnpj}): falha na origem (IsSuccess=false).");
                    continue;
                }

                try
                {
                    await ImportInsurerAsync(insurerId, insurerCatalog, nowUtc, cancellationToken);
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
        Guid insurerId, ImportedInsurerCatalog insurerCatalog, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var seenSourceIds = new HashSet<string>();

        foreach (var data in insurerCatalog.Modalities)
        {
            var importedGroupId = await UpsertGroupAsync(insurerId, data, cancellationToken);
            var imported = await UpsertModalityAsync(insurerId, data, importedGroupId, nowUtc, cancellationToken);
            seenSourceIds.Add(imported.SourceId);

            await TryMapByIdentifierAsync(imported, data, cancellationToken);
        }

        // RN-035: Modalidades Importadas Ativas que não vieram nesta importação bem-sucedida são desativadas.
        var active = await importedModalityRepository.ListActiveByInsurerAsync(insurerId, cancellationToken);

        foreach (var modality in active.Where(modality => !seenSourceIds.Contains(modality.SourceId)))
        {
            modality.Deactivate();
        }
    }

    private async Task<Guid> UpsertGroupAsync(
        Guid insurerId, ImportedModalityData data, CancellationToken cancellationToken)
    {
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

    /// <summary>RN-032: confirma automaticamente só quando o identificador do motor já aponta para uma Modalidade, no mesmo ramo.</summary>
    private async Task TryMapByIdentifierAsync(
        ImportedModality imported, ImportedModalityData data, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(data.EngineModalityId)
            || await modalityMappingRepository.HasConfirmedAsync(imported.Id, cancellationToken))
        {
            return;
        }

        var modalityId = await importedModalityRepository.FindConfirmedModalityIdByEngineAsync(
            data.EngineModalityId, data.Branch, cancellationToken);

        if (modalityId is not null)
        {
            await modalityMappingRepository.AddAsync(
                ModalityMapping.CreateByIdentifier(imported.Id, modalityId.Value), cancellationToken);
        }
    }

    private ICalculationEngine ResolveEngine(string calculationEngine)
    {
        var engine = Enum.Parse<ECalculationEngine>(calculationEngine);
        return serviceProvider.GetRequiredKeyedService<ICalculationEngine>(engine);
    }
}
