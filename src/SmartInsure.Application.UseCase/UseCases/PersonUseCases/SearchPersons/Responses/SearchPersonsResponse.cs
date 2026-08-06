namespace SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Responses;

/// <summary>
/// Resultado da busca (RN-013): sempre uma lista. <see cref="Notice"/> comunica consulta
/// sem dado na fonte (RN-004/RN-014) — não é erro, o fluxo solicitante conclui normalmente.
/// </summary>
public sealed record SearchPersonsResponse(
    IReadOnlyList<PersonSearchItemResponse> Items,
    string? Notice = null);

/// <summary>
/// Pessoa no resultado: tipo pelo nome estável (Natural/Legal); classificação da
/// Natureza Jurídica quando jurídica (RN-015); no contexto de tomador com CNPJ de
/// filial, a filial pré-selecionada — cadastrada e vinculada à matriz (RN-016/RN-101).
/// </summary>
public sealed record PersonSearchItemResponse(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    string Type,
    bool? IsPrivateSector,
    IReadOnlyList<string> Roles,
    PersonAddressResponse? MainAddress,
    string? PreSelectedBranchDocumentNumber = null,
    Guid? PreSelectedBranchId = null,
    IReadOnlyList<PersonAddressOptionResponse>? Addresses = null);

/// <summary>
/// Endereço identificado da Pessoa (RN-503): o corretor escolhe um deles como endereço do Segurado da
/// oferta, e é o <see cref="Id"/> que volta no contrato do Grupo de Cotação. <see cref="IsMain"/> marca
/// qual a tela pré-seleciona.
/// </summary>
public sealed record PersonAddressOptionResponse(
    Guid Id,
    bool IsMain,
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);

public sealed record PersonAddressResponse(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);
