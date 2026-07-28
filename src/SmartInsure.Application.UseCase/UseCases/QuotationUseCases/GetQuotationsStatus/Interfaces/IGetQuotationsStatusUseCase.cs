using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationsStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationsStatus.Responses;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationsStatus.Interfaces;

public interface IGetQuotationsStatusUseCase : IUseCase<GetQuotationsStatusRequest, QuotationsStatusResponse>
{
}
