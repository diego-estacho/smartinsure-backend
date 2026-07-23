namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ListCreditInquiries.Requests;

/// <summary>RN-031: listagem paginada de histórico de consultas com filtros opcionais.</summary>
public sealed record ListCreditInquiriesRequest
{
    public Guid? BrokerageId { get; init; }
    public string? PolicyHolderCnpj { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
