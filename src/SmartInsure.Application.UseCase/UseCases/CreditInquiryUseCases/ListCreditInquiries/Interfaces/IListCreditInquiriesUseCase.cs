using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ListCreditInquiries.Requests;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ListCreditInquiries.Responses;

namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ListCreditInquiries.Interfaces;

public interface IListCreditInquiriesUseCase : IUseCase<ListCreditInquiriesRequest, ListCreditInquiriesResponse>
{
}
