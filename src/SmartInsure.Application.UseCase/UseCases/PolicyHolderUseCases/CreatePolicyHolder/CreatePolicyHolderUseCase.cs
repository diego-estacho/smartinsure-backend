using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolder.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolder.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolder.Responses;
using SmartInsure.Application.UseCase.Services.PersonImports;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolder;

/// <summary>RN-025 — cria Tomador por CNPJ como Pessoa jurídica (matriz) com papel PolicyHolder ativo.</summary>
public sealed class CreatePolicyHolderUseCase(
    IPersonRepository personRepository,
    IPersonBureauImporter personBureauImporter,
    IUnitOfWork unitOfWork) : ICreatePolicyHolderUseCase
{
    private const string NotFoundMessage = "CNPJ não localizado na fonte de dados cadastrais.";

    public async Task<CreatePolicyHolderResponse> ExecuteAsync(
        CreatePolicyHolderRequest request,
        CancellationToken cancellationToken)
    {
        var cnpj = CnpjValidator.Normalize(request.Cnpj);

        // RN-016: tomador é sempre a matriz.
        var headquartersCnpj = CnpjValidator.IsHeadquarters(cnpj)
            ? cnpj
            : CnpjValidator.HeadquartersOf(cnpj);

        var existing = await personRepository.GetTrackedByDocumentNumberAsync(
            headquartersCnpj, cancellationToken);
        if (existing is not null)
        {
            return await CreateFromExistingAsync(existing, cancellationToken);
        }

        var imported = await personBureauImporter.ImportLegalPersonAsync(
            headquartersCnpj, EPersonRole.PolicyHolder, cancellationToken);
        if (imported is null)
        {
            throw new BusinessRuleException(NotFoundMessage);
        }

        await personRepository.AddAsync(imported.Person, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return await BuildResponseAsync(imported.Person.Id, cancellationToken);
    }

    private async Task<CreatePolicyHolderResponse> CreateFromExistingAsync(
        Person person,
        CancellationToken cancellationToken)
    {
        if (person.Type != EPersonType.J)
        {
            throw new BusinessRuleException("O tomador deve ser uma Pessoa jurídica.");
        }

        if (person.GetRole(EPersonRole.PolicyHolder) is not null)
        {
            throw new ConflictException("Tomador já cadastrado.");
        }

        person.AssignRole(EPersonRole.PolicyHolder);
        await unitOfWork.CommitAsync(cancellationToken);

        return await BuildResponseAsync(person.Id, cancellationToken);
    }

    private async Task<CreatePolicyHolderResponse> BuildResponseAsync(
        Guid personId,
        CancellationToken cancellationToken)
    {
        var policyHolder = await personRepository.GetPolicyHolderByIdAsync(
            personId, cancellationToken)
            ?? throw new NotFoundException("Tomador não encontrado.");

        var mainAddress = policyHolder.Addresses.FirstOrDefault(a => a.IsMain);

        return new CreatePolicyHolderResponse(
            policyHolder.Id,
            policyHolder.DocumentNumber,
            policyHolder.Name,
            policyHolder.SocialName,
            policyHolder.LegalNatureCode,
            policyHolder.LegalNatureDescription,
            policyHolder.IsPrivateSector,
            mainAddress is null
                ? null
                : new ImportedPolicyHolderAddressResponse(
                    mainAddress.ZipCode,
                    mainAddress.Street,
                    mainAddress.Number,
                    mainAddress.Complement,
                    mainAddress.Neighborhood,
                    mainAddress.City,
                    mainAddress.State));
    }
}
