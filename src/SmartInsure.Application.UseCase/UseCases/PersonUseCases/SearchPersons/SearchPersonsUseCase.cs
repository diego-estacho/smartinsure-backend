using SmartInsure.Application.UseCase.Services.PersonImports;
using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Requests;
using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons;

/// <summary>
/// RN-013: busca por trecho de nome (nome/nome social) ou documento; pessoa já
/// cadastrada vem da base, sem Birô e sem atualização. RN-014: CNPJ não cadastrado é
/// importado do Birô uma única vez. RN-016: no contexto de tomador só matriz; CNPJ de
/// filial resolve a matriz com a filial pré-selecionada.
/// </summary>
public sealed class SearchPersonsUseCase(
    IPersonRepository personRepository,
    IPersonBureauImporter personBureauImporter,
    IUnitOfWork unitOfWork) : ISearchPersonsUseCase
{
    private const string NotFoundNotice = "CNPJ não localizado na fonte de dados cadastrais.";

    public async Task<SearchPersonsResponse> ExecuteAsync(
        SearchPersonsRequest request,
        CancellationToken cancellationToken)
    {
        var role = Enum.Parse<EPersonRole>(request.Role);
        var headquartersOnly = role == EPersonRole.PolicyHolder;

        var digits = CnpjValidator.Normalize(request.Term);
        var documentNumber = digits.Length is 11 or 14 ? digits : null;
        var cnpj = digits.Length == 14 ? digits : null;

        var found = await personRepository.SearchByNameOrDocumentAsync(
            request.Term.Trim(), documentNumber, headquartersOnly, cancellationToken);

        if (found.Count > 0)
        {
            // RN-017: só a devolução por documento vincula papel — busca por nome é exploratória.
            if (documentNumber is not null)
            {
                var matched = await AssignRoleByDocumentAsync(documentNumber, role, cancellationToken);

                if (matched)
                {
                    return new SearchPersonsResponse(
                        [.. found.Select(item => item.DocumentNumber == documentNumber
                            ? MapItem(item, null, role)
                            : MapItem(item))]);
                }
            }

            return new SearchPersonsResponse([.. found.Select(item => MapItem(item))]);
        }

        // RN-013: termo que não é CNPJ (inclusive CPF) e sem correspondência não vai ao Birô.
        if (cnpj is null)
        {
            return new SearchPersonsResponse([]);
        }

        // RN-016: tomador com CNPJ de filial resolve a matriz, com a filial pré-selecionada.
        if (headquartersOnly && !CnpjValidator.IsHeadquarters(cnpj))
        {
            return await ResolveHeadquartersAsync(cnpj, role, cancellationToken);
        }

        var imported = await ImportFromBureauAsync(cnpj, role, cancellationToken);

        return imported is null
            ? new SearchPersonsResponse([], NotFoundNotice)
            : new SearchPersonsResponse([MapItem(imported)]);
    }

    private async Task<SearchPersonsResponse> ResolveHeadquartersAsync(
        string branchCnpj,
        EPersonRole role,
        CancellationToken cancellationToken)
    {
        var headquartersCnpj = CnpjValidator.HeadquartersOf(branchCnpj);

        var existing = await personRepository.GetByDocumentNumberAsync(
            headquartersCnpj, cancellationToken);

        if (existing is not null)
        {
            await AssignRoleByDocumentAsync(headquartersCnpj, role, cancellationToken);

            return new SearchPersonsResponse([MapItem(existing, branchCnpj, role)]);
        }

        var imported = await ImportFromBureauAsync(headquartersCnpj, role, cancellationToken);

        return imported is null
            ? new SearchPersonsResponse([], NotFoundNotice)
            : new SearchPersonsResponse([MapItem(imported, branchCnpj)]);
    }

    /// <summary>RN-017: vincula o papel via change tracker; idempotente na entidade.</summary>
    private async Task<bool> AssignRoleByDocumentAsync(
        string documentNumber,
        EPersonRole role,
        CancellationToken cancellationToken)
    {
        var person = await personRepository.GetTrackedByDocumentNumberAsync(
            documentNumber, cancellationToken);

        if (person is null)
        {
            return false;
        }

        person.AssignRole(role);
        await unitOfWork.CommitAsync(cancellationToken);

        return true;
    }

    private async Task<PersonSearchItemDto?> ImportFromBureauAsync(
        string cnpj,
        EPersonRole role,
        CancellationToken cancellationToken)
    {
        var imported = await personBureauImporter.ImportLegalPersonAsync(
            cnpj, role, cancellationToken);
        if (imported is null)
        {
            return null;
        }

        var person = imported.Person;
        await personRepository.AddAsync(person, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        var mainAddress = person.Addresses.Single(address => address.IsMain);

        return new PersonSearchItemDto(
            person.Id,
            person.DocumentNumber,
            person.Name,
            person.SocialName,
            person.Type.ToString(),
            imported.IsPrivateSector,
            [role.ToString()],
            new PersonMainAddressDto(
                mainAddress.ZipCode,
                mainAddress.Street,
                mainAddress.Number,
                mainAddress.Complement,
                mainAddress.Neighborhood,
                mainAddress.City,
                mainAddress.State));
    }

    private static PersonSearchItemResponse MapItem(
        PersonSearchItemDto item,
        string? preSelectedBranchDocumentNumber = null,
        EPersonRole? ensuredRole = null)
        => new(
            item.Id,
            item.DocumentNumber,
            item.Name,
            item.SocialName,
            item.Type,
            item.IsPrivateSector,
            EnsureRole(item.Roles, ensuredRole),
            item.MainAddress is null
                ? null
                : new PersonAddressResponse(
                    item.MainAddress.ZipCode,
                    item.MainAddress.Street,
                    item.MainAddress.Number,
                    item.MainAddress.Complement,
                    item.MainAddress.Neighborhood,
                    item.MainAddress.City,
                    item.MainAddress.State),
            preSelectedBranchDocumentNumber);

    private static IReadOnlyList<string> EnsureRole(
        IReadOnlyList<string> roles, EPersonRole? ensuredRole)
    {
        if (ensuredRole is null || roles.Contains(ensuredRole.Value.ToString()))
        {
            return roles;
        }

        return [.. roles, ensuredRole.Value.ToString()];
    }
}
