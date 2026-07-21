using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.GetCreditInquiry.Requests;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.GetCreditInquiry.Responses;

namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.GetCreditInquiry.Interfaces;

public interface IGetCreditInquiryUseCase : IUseCase<GetCreditInquiryRequest, GetCreditInquiryResponse>
{
}
