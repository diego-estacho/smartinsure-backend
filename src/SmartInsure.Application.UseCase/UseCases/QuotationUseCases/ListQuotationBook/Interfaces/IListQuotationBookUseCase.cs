using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Responses;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Interfaces;

public interface IListQuotationBookUseCase : IUseCase<ListQuotationBookRequest, QuotationBookResponse>
{
}
