using SmartInsure.Application.UseCase.Services.PersonImports;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage;

/// <summary>
/// RN-019 — cria a Corretora na confirmação: garante a Pessoa jurídica (importando do Birô quando
/// nova), adiciona o papel Corretor com a situação escolhida e grava os dados complementares. A
/// consulta prévia do CNPJ é somente leitura (RN-052) — nada é gravado antes desta confirmação.
/// </summary>
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
            // RN-019: Pessoa jurídica que já é Corretora não é recriada.
            if (existing.GetRole(EPersonRole.Broker) is not null)
            {
                throw new ConflictException("Corretora já cadastrada.");
            }

            existing.SetUpBrokerage(
                request.ActivateOnSave,
                request.SocialName,
                request.ContactEmail,
                request.ContactPhone,
                request.ResponsibleName);

            await unitOfWork.CommitAsync(cancellationToken);
            return await BuildResponseAsync(existing.Id, cancellationToken);
        }

        var imported = await personBureauImporter.ImportLegalPersonAsync(
            cnpj, EPersonRole.Broker, cancellationToken)
            ?? throw new BusinessRuleException(NotFoundMessage);

        imported.Person.SetUpBrokerage(
            request.ActivateOnSave,
            request.SocialName,
            request.ContactEmail,
            request.ContactPhone,
            request.ResponsibleName);

        await personRepository.AddAsync(imported.Person, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return await BuildResponseAsync(imported.Person.Id, cancellationToken);
    }

    private async Task<CreateBrokerageResponse> BuildResponseAsync(
        Guid personId,
        CancellationToken cancellationToken)
    {
        var brokerage = await personRepository.GetBrokerageByIdAsync(personId, cancellationToken)
            ?? throw new NotFoundException("Corretora não encontrada.");

        return CreateBrokerageResponse.From(brokerage);
    }
}
