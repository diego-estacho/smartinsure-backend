using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SubmitQuotationTerms.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SubmitQuotationTerms.Responses;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SubmitQuotationTerms.Interfaces;

public interface ISubmitQuotationTermsUseCase : IUseCase<SubmitQuotationTermsRequest, SubmitQuotationTermsResponse>
{
}
