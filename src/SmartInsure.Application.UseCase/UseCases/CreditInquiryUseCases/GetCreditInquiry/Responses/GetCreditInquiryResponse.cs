using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Responses;

namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.GetCreditInquiry.Responses;

/// <summary>RN-031: detalhe completo de uma consulta de crédito do histórico.</summary>
public sealed record GetCreditInquiryResponse(
    Guid CreditInquiryId,
    DateTime QueriedAt,
    string PolicyHolderCnpj,
    string? PolicyHolderName,
    CreditInquirySummary Summary,
    IReadOnlyList<CreditInquiryResultResponse> Results);
