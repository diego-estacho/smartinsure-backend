using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Requests;
using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons;

/// <summary>
/// RN-013: busca por trecho de nome (razão social/fantasia) ou CNPJ; pessoa já cadastrada
/// vem da base, sem Birô e sem atualização. RN-014: CNPJ não cadastrado é importado do
/// Birô uma única vez. RN-016: no contexto de tomador só matriz; CNPJ de filial resolve a
/// matriz com a filial pré-selecionada.
/// </summary>
public sealed class SearchPersonsUseCase(
    IPersonRepository personRepository,
    ILegalNatureRepository legalNatureRepository,
    IBureauProvider bureauProvider,
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
        var cnpj = digits.Length == 14 ? digits : null;

        var found = await personRepository.SearchByNameOrCnpjAsync(
            request.Term.Trim(), cnpj, headquartersOnly, cancellationToken);

        if (found.Count > 0)
        {
            return new SearchPersonsResponse([.. found.Select(item => MapItem(item))]);
        }

        // RN-013: termo que não é CNPJ e sem correspondência não vai ao Birô.
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

        var existing = await personRepository.GetByCnpjAsync(headquartersCnpj, cancellationToken);

        if (existing is not null)
        {
            return new SearchPersonsResponse([MapItem(existing, branchCnpj)]);
        }

        var imported = await ImportFromBureauAsync(headquartersCnpj, role, cancellationToken);

        return imported is null
            ? new SearchPersonsResponse([], NotFoundNotice)
            : new SearchPersonsResponse([MapItem(imported, branchCnpj)]);
    }

    private async Task<PersonSearchItemDto?> ImportFromBureauAsync(
        string cnpj,
        EPersonRole role,
        CancellationToken cancellationToken)
    {
        var personType = role == EPersonRole.Insured ? "Segurado" : "Tomador";

        var complement = await bureauProvider.GetPersonComplementAsync(
            cnpj, personType, EBureau.ReceitaWS, cancellationToken);

        // RN-004/RN-014: consulta sem dado não bloqueia — nada é gravado.
        if (complement is null || string.IsNullOrWhiteSpace(complement.Name))
        {
            return null;
        }

        var legalNature = await ResolveLegalNatureAsync(complement, cancellationToken);

        var person = Person.Create(
            cnpj, complement.Name, complement.TradeName, legalNature.Id);

        person.AddMainAddress(
            complement.ZipCode,
            complement.Street,
            complement.Number,
            complement.AddressComplement,
            complement.District,
            complement.City,
            complement.State);

        await personRepository.AddAsync(person, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        var mainAddress = person.Addresses.Single(address => address.IsMain);

        return new PersonSearchItemDto(
            person.Id,
            person.Cnpj,
            person.CorporateName,
            person.TradeName,
            legalNature.IsPrivate,
            new PersonMainAddressDto(
                mainAddress.ZipCode,
                mainAddress.Street,
                mainAddress.Number,
                mainAddress.Complement,
                mainAddress.Neighborhood,
                mainAddress.City,
                mainAddress.State));
    }

    private async Task<LegalNature> ResolveLegalNatureAsync(
        BureauPersonComplement complement,
        CancellationToken cancellationToken)
    {
        var code = new string([.. (complement.LegalNature ?? string.Empty).Where(char.IsDigit)]);

        var legalNature = string.IsNullOrEmpty(code)
            ? null
            : await legalNatureRepository.GetByCodeAsync(code, cancellationToken);

        // RN-014/RN-015: natureza jurídica não catalogada recusa a importação.
        return legalNature
            ?? throw new BusinessRuleException(
                "A natureza jurídica retornada pela fonte não está catalogada na plataforma.");
    }

    private static PersonSearchItemResponse MapItem(
        PersonSearchItemDto item,
        string? preSelectedBranchCnpj = null)
        => new(
            item.Id,
            item.Cnpj,
            item.CorporateName,
            item.TradeName,
            item.IsPrivateSector,
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
            preSelectedBranchCnpj);
}
