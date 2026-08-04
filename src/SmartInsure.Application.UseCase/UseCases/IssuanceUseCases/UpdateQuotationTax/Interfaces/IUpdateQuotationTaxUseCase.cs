using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.UpdateQuotationTax.Requests;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.UpdateQuotationTax.Responses;

namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.UpdateQuotationTax.Interfaces;

/// <summary>RN-504: submete a taxa nova à Seguradora e aplica o retorno na Cotação escolhida.</summary>
public interface IUpdateQuotationTaxUseCase
    : IUseCase<UpdateQuotationTaxRequest, UpdateQuotationTaxResponse>
{
}
