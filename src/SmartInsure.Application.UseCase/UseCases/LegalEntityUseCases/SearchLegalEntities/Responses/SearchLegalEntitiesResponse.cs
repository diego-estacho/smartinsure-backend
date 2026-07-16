namespace SmartInsure.Application.UseCase.UseCases.LegalEntityUseCases.SearchLegalEntities.Responses;

/// <summary>
/// Resultado da busca (RN-013): sempre uma lista. <see cref="Notice"/> comunica consulta
/// sem dado na fonte (RN-004/RN-014) — não é erro, o fluxo solicitante conclui normalmente.
/// </summary>
public sealed record SearchLegalEntitiesResponse(
    IReadOnlyList<LegalEntitySearchItemResponse> Items,
    string? Notice = null);

/// <summary>
/// Pessoa Jurídica no resultado, com a classificação da Natureza Jurídica (RN-015) e,
/// no contexto de tomador com CNPJ de filial, a filial pré-selecionada (RN-016).
/// </summary>
public sealed record LegalEntitySearchItemResponse(
    Guid Id,
    string Cnpj,
    string CorporateName,
    string? TradeName,
    bool IsPrivateSector,
    LegalEntityAddressResponse? MainAddress,
    string? PreSelectedBranchCnpj = null);

public sealed record LegalEntityAddressResponse(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);
