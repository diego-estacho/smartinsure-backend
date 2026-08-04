namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.GetQuotationGroup.Responses;

/// <summary>
/// Estado persistido do Grupo de Cotação para reidratar o wizard ao atualizar a página: os escalares do
/// pedido (risco/escopo/coberturas), a Cotação escolhida (RN-059) e o Tomador/Segurado/Modalidade já
/// resolvidos (nome, documento e endereço principal estruturado) — o cliente não precisa refazer buscas.
/// </summary>
public sealed record GetQuotationGroupResponse(
    Guid Id,
    Guid PolicyHolderId,
    Guid InsuredId,
    Guid ModalityId,
    string ModalityName,
    decimal InsuredAmount,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    string ScopeMode,
    IReadOnlyList<Guid> InsurerIds,
    IReadOnlyList<Guid> AdditionalCoverageIds,
    string Status,
    Guid? SelectedQuotationId,
    QuotationGroupPersonResponse PolicyHolder,
    QuotationGroupPersonResponse Insured);

/// <summary>Tomador/Segurado resolvido para o resumo do wizard: identidade + endereço principal estruturado.</summary>
public sealed record QuotationGroupPersonResponse(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    QuotationGroupPersonAddressResponse? MainAddress);

/// <summary>Endereço principal estruturado — o cliente formata para exibição (mesma forma da busca de Pessoas).</summary>
public sealed record QuotationGroupPersonAddressResponse(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);
