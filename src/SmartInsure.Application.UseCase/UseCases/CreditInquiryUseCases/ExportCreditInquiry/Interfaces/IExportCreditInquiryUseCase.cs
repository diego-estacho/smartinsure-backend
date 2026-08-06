using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExportCreditInquiry.Requests;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExportCreditInquiry.Responses;

namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExportCreditInquiry.Interfaces;

public interface IExportCreditInquiryUseCase
    : IUseCase<ExportCreditInquiryRequest, ExportCreditInquiryResponse>;
