using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Responses;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Interfaces;

public interface ISelectQuotationUseCase : IUseCase<SelectQuotationRequest, SelectQuotationResponse>
{
}
