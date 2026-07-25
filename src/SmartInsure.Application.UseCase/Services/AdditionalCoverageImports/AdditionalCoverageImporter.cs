using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.Services.AdditionalCoverageImports;

/// <summary>
/// Importação de Coberturas Adicionais Importadas (RN-042): por Seguradora Ativa e habilitada (dedup
/// por Seguradora, OPEN-09 — usa a primeira Habilitação Ativa), consulta o Motor de Cálculo resolvido
/// pela Habilitação uma vez por Modalidade Importada processável. Faz upsert da Cobertura Adicional
/// Importada por (Modalidade Importada + nome) (RN-041), confirma o ramo (RN-041), reconcilia
/// desativando o que não veio numa consulta bem-sucedida sem apagar e preservando o vínculo manual
/// (RN-043/RN-044), e isola a falha por modalidade, montando um resumo auditável (RN-045). Nunca cria
/// nem vincula a Cobertura Adicional canônica — o vínculo é manual, na curadoria (RN-043).
/// </summary>
public sealed class AdditionalCoverageImporter(
    IBrokerageInsurerEnablementRepository enablementRepository,
    IImportedModalityRepository importedModalityRepository,
    IImportedAdditionalCoverageRepository importedAdditionalCoverageRepository,
    IServiceProvider serviceProvider,
    IUnitOfWork unitOfWork) : IAdditionalCoverageImporter
{
    public async Task<AdditionalCoverageImportSummary> RunAsync(DateTime nowUtc, CancellationToken cancellationToken)
    {
        var enablements = await enablementRepository.ListActiveForImportAsync(cancellationToken);

        var processedInsurers = new HashSet<Guid>();
        var failures = new List<string>();
        int processed = 0, succeeded = 0, failed = 0;

        foreach (var enablement in enablements)
        {
            // OPEN-09: Seguradora habilitada por várias Corretoras é consultada uma vez (primeira Ativa).
            if (!processedInsurers.Add(enablement.InsurerId))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(enablement.InsurerReferenceExternalId))
            {
                failures.Add($"Seguradora {enablement.InsurerId}: sem identificador de origem (InsuranceUniqueId).");
                continue;
            }

            var modalities = await importedModalityRepository.ListImportableForCoverageAsync(
                enablement.InsurerId, cancellationToken);
            var engine = ResolveEngine(enablement.CalculationEngine);

            foreach (var modality in modalities)
            {
                processed++;

                ImportedAdditionalCoverageResult result;

                try
                {
                    result = await engine.GetAdditionalCoveragesAsync(
                        enablement.ConnectionParameters,
                        enablement.BrokerCnpj,
                        enablement.InsurerReferenceExternalId!,
                        modality.ModalityName,
                        modality.ModalityGroupType,
                        cancellationToken);
                }
                catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
                {
                    // RN-045: falha de transporte isola a modalidade — não desativa suas coberturas importadas.
                    failed++;
                    failures.Add($"Modalidade {modality.ModalityName}: {exception.Message}");
                    continue;
                }

                if (!result.IsSuccess)
                {
                    // RN-045: erro de envelope/corpo nulo — não desativa (RN-044).
                    failed++;
                    failures.Add($"Modalidade {modality.ModalityName}: {result.ErrorMessage}");
                    continue;
                }

                try
                {
                    await UpsertAndReconcileAsync(modality, result, nowUtc, cancellationToken);
                    await unitOfWork.CommitAsync(cancellationToken);
                    succeeded++;
                }
                catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
                {
                    failed++;
                    failures.Add($"Modalidade {modality.ModalityName}: {exception.Message}");
                }
            }
        }

        return new AdditionalCoverageImportSummary(processed, succeeded, failed, failures);
    }

    private async Task UpsertAndReconcileAsync(
        ImportableModalityForCoverageDto modality,
        ImportedAdditionalCoverageResult result,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var seenNames = new HashSet<string>();

        foreach (var coverage in result.Coverages)
        {
            // RN-041: o ramo confirma o vínculo à Modalidade Importada — cobertura de ramo divergente é descartada.
            if (coverage.Branch != modality.Branch)
            {
                continue;
            }

            var name = coverage.Name.Trim();

            // RN-041: cobertura sem nome é ignorada.
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            // RN-041: duplicata (mesmo nome) no retorno é a mesma cobertura — a primeira vence.
            if (!seenNames.Add(name))
            {
                continue;
            }

            var existing = await importedAdditionalCoverageRepository.GetByImportedModalityAndNameAsync(
                modality.ImportedModalityId, name, cancellationToken);

            if (existing is null)
            {
                var created = ImportedAdditionalCoverage.Create(
                    modality.ImportedModalityId, name, coverage.SourceUniqueId,
                    coverage.InsuredAmountCalculationType, coverage.AllowManualEdit, nowUtc);
                await importedAdditionalCoverageRepository.AddAsync(created, cancellationToken);
            }
            else
            {
                // RN-043: UpdateFromSource preserva o vínculo manual (AdditionalCoverageId).
                existing.UpdateFromSource(
                    coverage.SourceUniqueId, coverage.InsuredAmountCalculationType,
                    coverage.AllowManualEdit, nowUtc);
            }
        }

        // RN-044: Coberturas Adicionais Importadas Ativas que não vieram nesta consulta bem-sucedida são desativadas.
        var active = await importedAdditionalCoverageRepository.ListActiveByImportedModalityAsync(
            modality.ImportedModalityId, cancellationToken);

        foreach (var coverage in active.Where(coverage => !seenNames.Contains(coverage.Name)))
        {
            coverage.Deactivate();
        }
    }

    private ICalculationEngine ResolveEngine(string calculationEngine)
    {
        var engine = Enum.Parse<ECalculationEngine>(calculationEngine);
        return serviceProvider.GetRequiredKeyedService<ICalculationEngine>(engine);
    }
}
