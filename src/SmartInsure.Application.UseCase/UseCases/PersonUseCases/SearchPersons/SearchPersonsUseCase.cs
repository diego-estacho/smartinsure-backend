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
/// filial resolve a matriz com a filial pré-selecionada. RN-101: essa resolução delega a
/// IBranchRegistrar — cadastro de matriz/Filial pelo Birô e vínculo entre elas passam a
/// ser responsabilidade do registrar; este use case só reage ao BranchRegistration
/// devolvido.
/// </summary>
public sealed class SearchPersonsUseCase(
    IPersonRepository personRepository,
    IPersonBureauImporter personBureauImporter,
    IBranchRegistrar branchRegistrar,
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
        // RN-101: cadastra matriz e Filial pelo Birô e vincula; a Filial deixa de ser
        // indicação transitória (RN-016 revisada) e passa a existir como dado.
        var registration = await branchRegistrar.RegisterAsync(branchCnpj, cancellationToken);

        if (registration is null)
        {
            return new SearchPersonsResponse([], NotFoundNotice);
        }

        var headquartersCnpj = CnpjValidator.HeadquartersOf(branchCnpj);

        // RN-017: BranchRegistrar nunca atribui Papel (ADR-101) — permanece com o caller.
        await AssignRoleByDocumentAsync(headquartersCnpj, role, cancellationToken);

        var headquarters = await personRepository.GetByDocumentNumberAsync(
            headquartersCnpj, cancellationToken);

        // Defensivo: registration não nulo já implica que o registrar comitou a matriz
        // nesta mesma DbContext, então headquarters nulo aqui não é alcançável na
        // prática — fallback mantido apenas por segurança, não é um bug a "corrigir".
        // RN-016 (Casos limite): Filial não localizada no Birô devolve a matriz SEM Filial
        // pré-selecionada — o documento da Filial só acompanha a resposta quando ela de fato
        // existe (registration.BranchId não nulo); do contrário ficaria com o documento
        // preenchido e o id nulo, uma pré-seleção inconsistente que o contrato não deveria expor.
        return headquarters is null
            ? new SearchPersonsResponse([], NotFoundNotice)
            : new SearchPersonsResponse(
                [MapItem(
                    headquarters,
                    registration.BranchId is null ? null : branchCnpj,
                    role,
                    registration.BranchId)],
                registration.Notice);
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
            cnpj, role, assignRole: true, cancellationToken);
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
                mainAddress.State),
            // RN-503: Pessoa recém-importada do Birô tem só o endereço principal — a lista já sai com ele
            // para a tela poder pré-selecionar sem uma segunda consulta.
            person.Addresses
                .OrderByDescending(address => address.IsMain)
                .Select(address => new PersonAddressItemDto(
                    address.Id,
                    address.IsMain,
                    address.ZipCode,
                    address.Street,
                    address.Number,
                    address.Complement,
                    address.Neighborhood,
                    address.City,
                    address.State))
                .ToList());
    }

    private static PersonSearchItemResponse MapItem(
        PersonSearchItemDto item,
        string? preSelectedBranchDocumentNumber = null,
        EPersonRole? ensuredRole = null,
        Guid? preSelectedBranchId = null)
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
            preSelectedBranchDocumentNumber,
            preSelectedBranchId,
            // RN-503: a lista de endereços com Id acompanha a Pessoa — é dela que o corretor escolhe o
            // endereço do Segurado da oferta, sem uma segunda chamada.
            item.Addresses
                .Select(address => new PersonAddressOptionResponse(
                    address.Id,
                    address.IsMain,
                    address.ZipCode,
                    address.Street,
                    address.Number,
                    address.Complement,
                    address.Neighborhood,
                    address.City,
                    address.State))
                .ToList());

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
