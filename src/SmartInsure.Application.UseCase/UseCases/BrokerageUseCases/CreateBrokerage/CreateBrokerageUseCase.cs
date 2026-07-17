using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Responses;
using SmartInsure.Application.UseCase.Services.PersonImports;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage;

/// <summary>RN-019 — cria Corretora por CNPJ como Pessoa jurídica com papel Corretor ativo.</summary>
public sealed class CreateBrokerageUseCase(
    IPersonRepository personRepository,
    IPersonBureauImporter personBureauImporter,
    IUnitOfWork unitOfWork) : ICreateBrokerageUseCase
{
    private const string NotFoundMessage = "CNPJ não localizado na fonte de dados cadastrais.";

    public async Task<CreateBrokerageResponse> ExecuteAsync(
        CreateBrokerageRequest request,
        CancellationToken cancellationToken)
    {
        var cnpj = CnpjValidator.Normalize(request.Cnpj);

        var existing = await personRepository.GetTrackedByDocumentNumberAsync(cnpj, cancellationToken);
        if (existing is not null)
        {
            return await CreateFromExistingAsync(existing, cancellationToken);
        }

        var imported = await personBureauImporter.ImportLegalPersonAsync(
            cnpj, EPersonRole.Broker, cancellationToken);
        if (imported is null)
        {
            throw new BusinessRuleException(NotFoundMessage);
        }

        await personRepository.AddAsync(imported.Person, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return await BuildResponseAsync(imported.Person.Id, cancellationToken);
    }

    private async Task<CreateBrokerageResponse> CreateFromExistingAsync(
        Person person,
        CancellationToken cancellationToken)
    {
        if (person.Type != EPersonType.J)
        {
            throw new BusinessRuleException("A corretora deve ser uma Pessoa jurídica.");
        }

        if (person.GetRole(EPersonRole.Broker) is not null)
        {
            throw new ConflictException("Corretora já cadastrada.");
        }

        person.AssignRole(EPersonRole.Broker);
        await unitOfWork.CommitAsync(cancellationToken);

        return await BuildResponseAsync(person.Id, cancellationToken);
    }

    private async Task<CreateBrokerageResponse> BuildResponseAsync(
        Guid personId,
        CancellationToken cancellationToken)
    {
        var brokerage = await personRepository.GetBrokerageByIdAsync(personId, cancellationToken)
            ?? throw new NotFoundException("Corretora não encontrada.");

        return new CreateBrokerageResponse(
            brokerage.Id,
            brokerage.DocumentNumber,
            brokerage.Name,
            brokerage.SocialName,
            brokerage.LegalNatureCode,
            brokerage.LegalNatureDescription,
            brokerage.IsPrivateSector,
            brokerage.Status,
            brokerage.MainAddress is null
                ? null
                : new ImportedBrokerageAddressResponse(
                    brokerage.MainAddress.ZipCode,
                    brokerage.MainAddress.Street,
                    brokerage.MainAddress.Number,
                    brokerage.MainAddress.Complement,
                    brokerage.MainAddress.Neighborhood,
                    brokerage.MainAddress.City,
                    brokerage.MainAddress.State));
    }
}
