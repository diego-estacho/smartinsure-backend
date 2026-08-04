namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.UpdateQuotationTax.Requests;

/// <summary>RN-504: ajuste da taxa da Cotação escolhida do Grupo, na etapa de emissão.</summary>
public sealed record UpdateQuotationTaxRequest
{
    public required Guid QuotationGroupId { get; init; }

    /// <summary>Taxa pretendida pelo corretor; a plataforma valida só o formato (RN-504).</summary>
    public required decimal Tax { get; init; }
}
