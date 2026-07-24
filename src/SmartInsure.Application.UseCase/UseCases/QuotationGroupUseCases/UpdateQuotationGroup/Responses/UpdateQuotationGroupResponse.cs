namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Responses;

/// <summary>Dados de saída do Grupo de Cotação atualizado (permanece em Rascunho).</summary>
public sealed record UpdateQuotationGroupResponse(
    Guid Id,
    Guid PolicyHolderId,
    Guid InsuredId,
    Guid ModalityId,
    decimal InsuredAmount,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    string ScopeMode,
    IReadOnlyList<Guid> InsurerIds,
    bool IncludesPenaltyCoverage,
    bool IncludesLaborCoverage,
    string Status);
