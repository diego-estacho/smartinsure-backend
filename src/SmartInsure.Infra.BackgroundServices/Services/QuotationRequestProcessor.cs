using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Channels;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.BackgroundServices.Options;

namespace SmartInsure.Infra.BackgroundServices.Services;

/// <summary>
/// Processa uma Cotação do fan-out (RN-057/RN-058): reconstrói o contexto do estado persistido
/// (reconciliador-safe), resolve a Habilitação e o motor, chama /Cotation com tempo-limite por
/// Seguradora e grava o resultado (obtida) ou a indisponibilidade isolada (falha). Idempotente:
/// só processa Cotações ainda em Requested. Escopo de DI próprio por item (ADR-050).
/// </summary>
public sealed class QuotationRequestProcessor(
    IQuotationRepository quotationRepository,
    IQuotationGroupRepository groupRepository,
    IBrokerageInsurerEnablementRepository enablementRepository,
    IInsurerRepository insurerRepository,
    IServiceProvider serviceProvider,
    IUnitOfWork unitOfWork,
    IOptions<QuotationFanOutOptions> options)
{
    public async Task ProcessAsync(QuotationRequestWorkItem workItem, CancellationToken cancellationToken)
    {
        var quotation = await quotationRepository.GetByIdAsync(workItem.QuotationId, cancellationToken);

        // Idempotência (ADR-050): já processada por outra execução/reconciliador — ignora.
        if (quotation is null || quotation.ProcessingStatus != EQuotationProcessingStatus.Requested)
        {
            return;
        }

        try
        {
            var context = await groupRepository.GetContextAsync(
                    workItem.QuotationGroupId, quotation.BrokerageId, cancellationToken)
                ?? throw new BusinessRuleException("Contexto da cotação indisponível.");

            if (string.IsNullOrWhiteSpace(context.ModalityGlobalId))
            {
                quotation.MarkFailed("Modalidade sem identificador global — não é possível cotar nesta Seguradora.");
            }
            else
            {
                await CotateAsync(quotation, workItem.InsurerId, context, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            // Tempo-limite por Seguradora (CTS local) — falha isolada, não derruba as demais (RN-057).
            quotation.MarkFailed("Tempo-limite excedido ao cotar a Seguradora.");
        }
        catch (CalculationEngineException exception)
        {
            quotation.MarkFailed($"Falha na integração com a Seguradora: {exception.Message}");
        }
        catch (BusinessRuleException exception)
        {
            quotation.MarkFailed(exception.Message);
        }

        await unitOfWork.CommitAsync(cancellationToken);
    }

    private async Task CotateAsync(
        Core.Entities.Quotation quotation,
        Guid insurerId,
        Core.Abstractions.Repositories.Dtos.QuotationContextDto context,
        CancellationToken cancellationToken)
    {
        var insurer = await insurerRepository.GetByIdAsync(insurerId, cancellationToken);

        if (insurer is null
            || insurer.Status != EInsurerStatus.Active
            || string.IsNullOrWhiteSpace(insurer.ReferenceExternalId))
        {
            quotation.MarkFailed("Seguradora inativa ou sem identificador externo configurado.");
            return;
        }

        var enablement = await enablementRepository.GetByPairAsync(quotation.BrokerageId, insurerId, cancellationToken);

        if (enablement is null || enablement.Status != EBrokerageInsurerEnablementStatus.Active)
        {
            quotation.MarkFailed("Seguradora não habilitada para a Corretora.");
            return;
        }

        var engine = serviceProvider.GetKeyedService<ICalculationEngine>(enablement.CalculationEngine)
            ?? throw new BusinessRuleException("O motor de cálculo da Habilitação não está disponível.");

        var request = new QuotationEngineRequest
        {
            BrokerCnpj = context.BrokerCnpj,
            PolicyHolderCnpj = context.PolicyHolderCnpj,
            InsuredCpfCnpj = context.InsuredCpfCnpj,
            InsuranceUniqueId = insurer.ReferenceExternalId,
            ModalityGlobalId = context.ModalityGlobalId!,
            ModalityName = context.ModalityName,
            InsuredAmount = context.InsuredAmount,
            CoverageStartDate = context.CoverageStartDate,
            CoverageEndDate = context.CoverageEndDate,
            IncludesPenaltyCoverage = context.IncludesPenaltyCoverage,
            IncludesLaborCoverage = context.IncludesLaborCoverage,
        };

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(options.Value.PerInsurerTimeoutSeconds));

        var result = await engine.RunQuotationAsync(enablement.ConnectionParameters, request, timeoutCts.Token);

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
            result.Reasons);
    }
}
