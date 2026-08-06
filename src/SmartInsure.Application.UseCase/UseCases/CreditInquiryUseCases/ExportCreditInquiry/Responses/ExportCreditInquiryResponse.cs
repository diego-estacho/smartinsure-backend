namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExportCreditInquiry.Responses;

public sealed record ExportCreditInquiryResponse(byte[] Content, string FileName, string ContentType);
