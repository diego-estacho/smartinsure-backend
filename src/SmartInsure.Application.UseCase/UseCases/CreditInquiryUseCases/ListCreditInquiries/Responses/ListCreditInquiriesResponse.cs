namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ListCreditInquiries.Responses;

/// <summary>Resposta paginada do histórico de Consultas de Crédito (RN-031).</summary>
public sealed record ListCreditInquiriesResponse(
    IReadOnlyList<CreditInquiryListItemResponse> Items,
    int Page,
    int PageSize,
    long Total);

/// <summary>Item resumido da listagem de histórico.</summary>
public sealed record CreditInquiryListItemResponse(
    Guid Id,
    Guid BrokerageId,
    string PolicyHolderCnpj,
    DateTime QueriedAt,
    int ResultsCount,
    int AvailableResults);
