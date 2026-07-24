namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Responses;

/// <summary>Dados de saída do Grupo de Cotação criado em Rascunho.</summary>
public sealed record CreateQuotationGroupResponse(
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
