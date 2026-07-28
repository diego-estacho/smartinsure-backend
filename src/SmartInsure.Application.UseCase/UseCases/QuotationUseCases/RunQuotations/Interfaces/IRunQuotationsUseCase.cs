using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Responses;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Interfaces;

public interface IRunQuotationsUseCase : IUseCase<RunQuotationsRequest, RunQuotationsResponse>
{
}
