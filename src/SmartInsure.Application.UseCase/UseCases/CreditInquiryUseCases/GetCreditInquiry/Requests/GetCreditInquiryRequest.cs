namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.GetCreditInquiry.Requests;

/// <summary>RN-031: recupera histórico de uma consulta de crédito específica.</summary>
public sealed record GetCreditInquiryRequest(Guid Id);
