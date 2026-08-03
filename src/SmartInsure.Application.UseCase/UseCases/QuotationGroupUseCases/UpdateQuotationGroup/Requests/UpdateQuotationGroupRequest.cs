namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Requests;

/// <summary>Atualização do Grupo de Cotação em Rascunho — mesmo id (RN-051). O id vem da rota.</summary>
/// <param name="BranchId">RN-102: estabelecimento cotado — Filial do Tomador; ausente limpa (volta a ser a matriz).</param>
public sealed record UpdateQuotationGroupRequest(
    Guid Id,
    Guid PolicyHolderId,
    Guid? BranchId,
    Guid InsuredId,
    Guid ModalityId,
    decimal InsuredAmount,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    string ScopeMode,
    IReadOnlyList<Guid> InsurerIds,
    bool IncludesPenaltyCoverage,
    bool IncludesLaborCoverage);
