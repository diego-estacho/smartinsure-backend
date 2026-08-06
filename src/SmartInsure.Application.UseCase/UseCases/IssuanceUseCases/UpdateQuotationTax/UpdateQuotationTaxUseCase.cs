using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.UpdateQuotationTax.Interfaces;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.UpdateQuotationTax.Requests;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.UpdateQuotationTax.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.UpdateQuotationTax;

/// <summary>
/// RN-504 — ajuste da taxa na emissão. A taxa pretendida é submetida à Seguradora, e o prêmio, a
/// comissão e as opções de parcelamento que ela devolve passam a valer na Cotação escolhida: a proposta
/// no provedor mudou de fato, então a Cotação é a fonte única e reflete isso (inclusive no leque).
/// A plataforma valida só o formato — o limite aceitável é da Seguradora (RN-511, OPEN-22). Recusa
/// preserva os valores anteriores. Não recota nada: taxa não é dado-base (RN-060). As irmãs seguem com
/// os valores originalmente cotados.
/// </summary>
public sealed class UpdateQuotationTaxUseCase(
    IQuotationRepository quotationRepository,
    IQuotationGroupRepository quotationGroupRepository,
    IBrokerageInsurerEnablementRepository enablementRepository,
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork,
    IServiceProvider serviceProvider) : IUpdateQuotationTaxUseCase
{
    public async Task<UpdateQuotationTaxResponse> ExecuteAsync(
        UpdateQuotationTaxRequest request,
        CancellationToken cancellationToken)
    {
        // Formato antes de qualquer chamada: taxa não positiva é erro de entrada, não veredito de risco.
        if (request.Tax <= 0m)
        {
            throw new BusinessRuleException("A taxa informada precisa ser maior que zero.");
        }

        var group = await quotationGroupRepository.GetByIdAsync(request.QuotationGroupId, cancellationToken)
            ?? throw new NotFoundException("Grupo de cotação não encontrado.");

        if (group.Status == EQuotationGroupStatus.EmissionRequested)
        {
            throw new BusinessRuleException(
                "A emissão desta oferta já foi solicitada — os valores não podem mais ser alterados.");
        }

        if (group.SelectedQuotationId is null)
        {
            throw new BusinessRuleException("Nenhuma cotação foi escolhida nesta oferta.");
        }

        var quotation = await quotationRepository.GetByIdAsync(group.SelectedQuotationId.Value, cancellationToken)
            ?? throw new NotFoundException("Cotação escolhida não encontrada.");

        if (quotation.Result != EQuotationResult.ReadyForEmission)
        {
            throw new BusinessRuleException("Só uma cotação pronta para emissão tem taxa a ajustar.");
        }

        if (string.IsNullOrWhiteSpace(quotation.ProposalExternalId))
        {
            throw new BusinessRuleException("A cotação escolhida não tem proposta na seguradora.");
        }

        // RN-504 (caso limite): taxa igual à vigente não é submetida — nada mudaria, e a Seguradora não
        // recalcula o que já vale. A Cotação volta como está, para quem chamou não precisar distinguir
        // este caso do recálculo.
        if (quotation.Tax == request.Tax)
        {
            return BuildResponse(quotation);
        }

        var (engine, connectionParameters) = await ResolveEngineAsync(quotation, cancellationToken);
        var brokerCnpj = await ResolveBrokerCnpjAsync(group, cancellationToken);

        ProposalFinancialDataResult result;

        try
        {
            result = await engine.UpdateProposalFinancialDataAsync(
                connectionParameters,
                new UpdateProposalFinancialDataInput
                {
                    BrokerCnpj = brokerCnpj,
                    ProposalExternalId = quotation.ProposalExternalId,
                    Tax = request.Tax,
                },
                cancellationToken);
        }
        catch (CalculationEngineException exception)
        {
            // Recusa (ou falha) da Seguradora: nada é aplicado — o corretor vê o motivo dela (RN-511).
            throw new BusinessRuleException(exception.Message);
        }

        quotation.ApplyFinancialData(
            result.Premium,
            result.Tax,
            result.CommissionPercentage,
            result.CommissionValue,
            result.InstallmentOptions,
            result.PossibleGracePeriodsInDays);

        quotationRepository.Update(quotation);
        await unitOfWork.CommitAsync(cancellationToken);

        return BuildResponse(quotation);
    }

    /// <summary>Os números que passam a valer na Cotação escolhida — a resposta é o espelho dela.</summary>
    private static UpdateQuotationTaxResponse BuildResponse(Quotation quotation)
        => new()
        {
            Premium = quotation.Premium,
            Tax = quotation.Tax,
            CommissionPercentage = quotation.CommissionPercentage,
            CommissionValue = quotation.CommissionValue,
            InstallmentOptions = quotation.ReadInstallmentOptions()
                .Select(option => new InstallmentOptionResponse(
                    option.Number, option.Description, option.Value, option.HasInterest))
                .ToList(),
            PossibleGracePeriodsInDays = quotation.ReadPossibleGracePeriodsInDays(),
        };

    /// <summary>
    /// RN-512: a Seguradora é acionada pela Habilitação que obteve a Cotação — nunca resolvida de novo,
    /// para que inativação posterior não mude o caminho de uma oferta já cotada.
    /// </summary>
    private async Task<(ICalculationEngine Engine, string? ConnectionParameters)> ResolveEngineAsync(
        Quotation quotation, CancellationToken cancellationToken)
    {
        if (quotation.BrokerageInsurerEnablementId is null)
        {
            throw new BusinessRuleException(
                "A cotação escolhida não registra a habilitação usada para obtê-la.");
        }

        var enablement = await enablementRepository.GetByIdAsync(
                quotation.BrokerageInsurerEnablementId.Value, cancellationToken)
            ?? throw new BusinessRuleException("A habilitação da seguradora não está mais disponível.");

        var engine = serviceProvider.GetKeyedService<ICalculationEngine>(enablement.CalculationEngine)
            ?? throw new BusinessRuleException("O motor de cálculo não está disponível na plataforma.");

        return (engine, enablement.ConnectionParameters);
    }

    private async Task<string> ResolveBrokerCnpjAsync(QuotationGroup group, CancellationToken cancellationToken)
    {
        if (group.BrokerageId is null)
        {
            throw new BusinessRuleException("A oferta não registra a corretora que a cotou.");
        }

        var brokerage = await personRepository.GetByIdAsync(group.BrokerageId.Value, cancellationToken)
            ?? throw new BusinessRuleException("Corretora da oferta não encontrada.");

        return brokerage.DocumentNumber;
    }
}
