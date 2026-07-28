namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationsStatus.Requests;

/// <summary>RN-057/ADR-051: acompanhamento do fan-out de um Grupo por polling.</summary>
public sealed record GetQuotationsStatusRequest(Guid QuotationGroupId);
