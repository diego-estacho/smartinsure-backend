namespace SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Responses;

/// <summary>
/// Resultado da busca (RN-013): sempre uma lista. <see cref="Notice"/> comunica consulta
/// sem dado na fonte (RN-004/RN-014) — não é erro, o fluxo solicitante conclui normalmente.
/// </summary>
public sealed record SearchPersonsResponse(
    IReadOnlyList<PersonSearchItemResponse> Items,
    string? Notice = null);

/// <summary>
/// Pessoa no resultado, com a classificação da Natureza Jurídica (RN-015) e,
/// no contexto de tomador com CNPJ de filial, a filial pré-selecionada (RN-016).
/// </summary>
public sealed record PersonSearchItemResponse(
    Guid Id,
    string Cnpj,
    string CorporateName,
    string? TradeName,
    bool IsPrivateSector,
    PersonAddressResponse? MainAddress,
    string? PreSelectedBranchCnpj = null);

public sealed record PersonAddressResponse(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);
