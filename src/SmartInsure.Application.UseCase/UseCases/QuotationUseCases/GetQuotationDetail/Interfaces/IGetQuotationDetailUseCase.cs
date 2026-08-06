using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationDetail.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationDetail.Responses;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationDetail.Interfaces;

public interface IGetQuotationDetailUseCase
    : IUseCase<GetQuotationDetailRequest, QuotationDetailResponse>;
