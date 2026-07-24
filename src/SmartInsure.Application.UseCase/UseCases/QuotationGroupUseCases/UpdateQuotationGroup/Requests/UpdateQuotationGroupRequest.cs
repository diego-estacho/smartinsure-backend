namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Requests;

/// <summary>Atualização do Grupo de Cotação em Rascunho — mesmo id (RN-051). O id vem da rota.</summary>
public sealed record UpdateQuotationGroupRequest(
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
    bool IncludesLaborCoverage);
