using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.Services.Quotations;

/// <summary>
/// Processa um item do fan-out (RN-057, consumidor ADR-050): carrega o risco do Grupo, resolve o motor
/// pela Habilitação, solicita a Cotação e persiste — <c>MarkObtained</c> no sucesso, <c>MarkFailed</c>
/// na falha isolada (nunca derruba as demais; sem retry automático). Idempotente: item cuja Cotação
/// não está mais em <c>Requested</c> (removida por recálculo ou já obtida) é ignorado.
/// </summary>
public sealed class QuotationRequestProcessor(
    IQuotationRepository quotationRepository,
    IQuotationGroupRepository quotationGroupRepository,
    IPersonRepository personRepository,
    IModalityRepository modalityRepository,
    IInsurerRepository insurerRepository,
    IBrokerageInsurerEnablementRepository enablementRepository,
    IUnitOfWork unitOfWork,
    IServiceProvider serviceProvider,
    IQuotationAdditionalCoverageResolver additionalCoverageResolver) : IQuotationRequestProcessor
{
    public async Task ProcessAsync(QuotationRequestWorkItem workItem, CancellationToken cancellationToken)
    {
        var quotation = await quotationRepository.GetByIdAsync(workItem.QuotationId, cancellationToken);

        // Idempotência: Cotação inexistente (recálculo removeu) ou já fora de Requested — nada a fazer.
        if (quotation is null || quotation.ProcessingStatus != EQuotationProcessingStatus.Requested)
        {
            return;
        }

        // ADR-050: carimba o lease e persiste ANTES de acionar o provedor — enquanto esta solicitação está
        // em voo (dentro do lease), o reconciliador não a reenfileira, evitando duplicar a proposta.
        quotation.BeginProcessing(DateTime.UtcNow);
        quotationRepository.Update(quotation);
        await unitOfWork.CommitAsync(cancellationToken);

        try
        {
            var resolved = await BuildRequestAsync(workItem, cancellationToken);
            var engine = ResolveEngine(resolved.Engine);

            // RN-106: a situação das Coberturas Adicionais é registrada ANTES de acionar a Seguradora,
            // para existir mesmo quando a Cotação virar Indisponível (RN-058) ou falhar na integração
            // (RN-057) — os dois caminhos abaixo não voltam aqui.
            quotation.RecordAdditionalCoverages(resolved.AdditionalCoverages.Items);

            var result = await engine.RunQuotationAsync(resolved.ConnectionParameters, resolved.Request, cancellationToken);

            quotation.MarkObtained(
                result.Result,
                result.AnalysisTrack,
                result.Premium,
                result.CommissionPercentage,
                result.CommissionValue,
                result.Tax,
                result.AvailableLimit,
                result.ProposalExternalId,
                result.ProposalNumber,
                result.RequiresCcg,
                result.CcgMaxLimitWithoutNeed,
                result.CcgSigned,
                result.Reasons,
                DateTime.UtcNow);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (QuotationSetupException setup)
        {
            // Pré-condição de dados (Seguradora inativa, sem id externo, sem modalidade global…):
            // indisponível com motivo, sem derrubar as demais (RN-057).
            quotation.MarkFailed(setup.Message, DateTime.UtcNow);
        }
        catch (CalculationEngineException engineFailure)
        {
            // Falha/timeout de integração → indisponível com motivo (RN-057), sem retry automático.
            quotation.MarkFailed($"Falha na integração: {engineFailure.Message}", DateTime.UtcNow);
        }

        quotationRepository.Update(quotation);
        await unitOfWork.CommitAsync(cancellationToken);
    }

    private async Task<ResolvedRequest> BuildRequestAsync(
        QuotationRequestWorkItem workItem, CancellationToken cancellationToken)
    {
        var group = await quotationGroupRepository.GetByIdAsync(workItem.QuotationGroupId, cancellationToken)
            ?? throw new QuotationSetupException("Grupo de Cotação não encontrado.");

        var enablement = await enablementRepository.GetByPairAsync(workItem.BrokerageId, workItem.InsurerId, cancellationToken)
            ?? throw new QuotationSetupException("Habilitação da Seguradora não encontrada.");

        if (enablement.Status != EBrokerageInsurerEnablementStatus.Active)
        {
            throw new QuotationSetupException("Habilitação da Seguradora está inativa.");
        }

        var insurer = await insurerRepository.GetByIdAsync(workItem.InsurerId, cancellationToken)
            ?? throw new QuotationSetupException("Seguradora não encontrada.");

        if (insurer.Status != EInsurerStatus.Active)
        {
            throw new QuotationSetupException("Seguradora está inativa no catálogo.");
        }

        if (string.IsNullOrWhiteSpace(insurer.ReferenceExternalId))
        {
            throw new QuotationSetupException("Identificador externo da Seguradora não configurado.");
        }

        var modality = await modalityRepository.GetByIdAsync(group.ModalityId, cancellationToken)
            ?? throw new QuotationSetupException("Modalidade não encontrada.");

        if (string.IsNullOrWhiteSpace(modality.GlobalModalityExternalId))
        {
            throw new QuotationSetupException("Modalidade sem id de Modalidade Global (não cotável no PLUG).");
        }

        var brokerage = await personRepository.GetByIdAsync(workItem.BrokerageId, cancellationToken)
            ?? throw new QuotationSetupException("Corretora não encontrada.");

        var policyHolder = await personRepository.GetByIdAsync(group.PolicyHolderId, cancellationToken)
            ?? throw new QuotationSetupException("Tomador não encontrado.");

        var insured = await personRepository.GetByIdAsync(group.InsuredId, cancellationToken)
            ?? throw new QuotationSetupException("Segurado não encontrado.");

        // RN-102: o CNPJ enviado à Seguradora é o do estabelecimento cotado — a Filial marcada
        // quando houver, senão a matriz (Tomador). Limite de Crédito e taxa continuam sempre da
        // matriz (RN-029) e não são afetados por esta resolução.
        var policyHolderCnpj = policyHolder.DocumentNumber;

        if (group.BranchPersonId is not null)
        {
            var branch = await personRepository.GetByIdAsync(group.BranchPersonId.Value, cancellationToken)
                ?? throw new QuotationSetupException("Filial do estabelecimento cotado não encontrada.");

            policyHolderCnpj = branch.DocumentNumber;
        }

        // RN-105/RN-106 (ADR-103): as canônicas escolhidas no Grupo viram os NOMES com que ESTA
        // Seguradora expõe as coberturas. O gateway recusa identificador de origem e recusa a
        // solicitação INTEIRA se receber cobertura não suportada — por isso nunca se envia superset.
        var additionalCoverages = await additionalCoverageResolver.ResolveAsync(
            workItem.InsurerId,
            group.ModalityId,
            group.AdditionalCoverages.Select(coverage => coverage.AdditionalCoverageId).ToList(),
            cancellationToken);

        var request = new QuotationRequestInput
        {
            // ADR-102: carregados só para o log de integração (QuotationIntegrationLog) gravado pelo motor.
            QuotationId = workItem.QuotationId,
            QuotationGroupId = workItem.QuotationGroupId,
            InsurerId = workItem.InsurerId,
            BrokerCnpj = brokerage.DocumentNumber,
            PolicyHolderCnpj = policyHolderCnpj,
            InsuredCpfCnpj = insured.DocumentNumber,
            InsuranceUniqueId = insurer.ReferenceExternalId,
            ModalityGlobalId = modality.GlobalModalityExternalId,
            ModalityName = modality.Name,
            InsuredAmount = group.InsuredAmount,
            StartDate = group.CoverageStartDate,
            EndDate = group.CoverageEndDate,
            AdditionalCoverages = additionalCoverages.NamesToSend,
        };

        return new ResolvedRequest(
            request, enablement.CalculationEngine, enablement.ConnectionParameters, additionalCoverages);
    }

    private ICalculationEngine ResolveEngine(ECalculationEngine engineType)
        => serviceProvider.GetKeyedService<ICalculationEngine>(engineType)
           ?? throw new QuotationSetupException("O motor de cálculo não está disponível na plataforma.");

    private sealed record ResolvedRequest(
        QuotationRequestInput Request,
        ECalculationEngine Engine,
        string? ConnectionParameters,
        AdditionalCoverageResolution AdditionalCoverages);

    /// <summary>Falha de pré-condição de dados de uma Seguradora — isolada, vira Cotação indisponível.</summary>
    private sealed class QuotationSetupException(string message) : Exception(message);
}
