using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationMinuta.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationMinuta.Responses;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationMinuta.Interfaces;

public interface IGetQuotationMinutaUseCase : IUseCase<GetQuotationMinutaRequest, QuotationMinutaResponse>
{
}
