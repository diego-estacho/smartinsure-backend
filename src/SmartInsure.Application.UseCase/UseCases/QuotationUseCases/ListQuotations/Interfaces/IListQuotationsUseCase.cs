using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations.Responses;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations.Interfaces;

public interface IListQuotationsUseCase : IUseCase<ListQuotationsRequest, ListQuotationsResponse>
{
}
