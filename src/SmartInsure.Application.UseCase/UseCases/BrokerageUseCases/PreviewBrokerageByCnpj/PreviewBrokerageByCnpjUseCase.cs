using SmartInsure.Application.UseCase.Services.PersonImports;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj;

/// <summary>
/// RN-052 (revisada) — consulta de um CNPJ para o cadastro de Corretora. Quando o CNPJ já existe na
/// base sem o papel Corretor e ainda está fresco (até 90 dias), reaproveita o cache sem novo custo
/// de Birô. Um CNPJ inédito é importado do Birô e a Pessoa jurídica é persistida SEM o papel Corretor
/// (a Corretora só nasce na confirmação — RN-019). RN-014 (import-once) é preservada: dados já
/// armazenados nunca são mutados; após 90 dias apenas reconsulta o Birô para EXIBIR, sem gravar.
/// </summary>
public sealed class PreviewBrokerageByCnpjUseCase(
    IPersonRepository personRepository,
    IPersonBureauImporter personBureauImporter,
    IUnitOfWork unitOfWork) : IPreviewBrokerageByCnpjUseCase
{
    private const string NotFoundMessage = "CNPJ não localizado na fonte de dados cadastrais.";
    private const double CacheFreshnessDays = 90;

    public async Task<BrokeragePreviewResponse> ExecuteAsync(
        PreviewBrokerageByCnpjRequest request,
        CancellationToken cancellationToken)
    {
        var cnpj = CnpjValidator.Normalize(request.Cnpj);

        var existing = await personRepository.FindBrokeragePreviewByDocumentAsync(cnpj, cancellationToken);
        if (existing is not null)
        {
            // Já é Corretora: sinaliza o cadastro existente. Nada é importado nem gravado.
            if (existing.HasBrokerRole)
            {
                return FromExisting(existing, alreadyRegistered: true, existing.PersonId);
            }

            // Cache fresco (RN-014): reaproveita sem novo custo de Birô e sem gravar.
            if ((DateTime.UtcNow - existing.LastUpdatedAt).TotalDays <= CacheFreshnessDays)
            {
                return FromExisting(existing, alreadyRegistered: false, existingBrokerageId: null);
            }

            // Cache vencido: reconsulta o Birô só para EXIBIR dados frescos (RN-014: sem gravar).
            var refreshed = await personBureauImporter.ImportLegalPersonAsync(
                cnpj, EPersonRole.Broker, assignRole: false, cancellationToken);

            return refreshed is null
                ? FromExisting(existing, alreadyRegistered: false, existingBrokerageId: null)
                : FromImported(refreshed);
        }

        // CNPJ inédito: importa do Birô e persiste a PJ SEM o papel Corretor (RN-052 revisada).
        var imported = await personBureauImporter.ImportLegalPersonAsync(
            cnpj, EPersonRole.Broker, assignRole: false, cancellationToken)
            ?? throw new BusinessRuleException(NotFoundMessage);

        await personRepository.AddAsync(imported.Person, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return FromImported(imported);
    }

    private static BrokeragePreviewResponse FromExisting(
        BrokeragePreviewDto existing,
        bool alreadyRegistered,
        Guid? existingBrokerageId)
        => new(
            existing.DocumentNumber,
            existing.Name,
            existing.SocialName,
            existing.LegalNatureCode,
            existing.LegalNatureName,
            existing.IsPrivateSector,
            alreadyRegistered,
            existingBrokerageId,
            MapAddress(existing.MainAddress));

    private static BrokeragePreviewResponse FromImported(PersonBureauImport imported)
    {
        var mainAddress = imported.Person.Addresses.FirstOrDefault(address => address.IsMain);

        return new BrokeragePreviewResponse(
            imported.Person.DocumentNumber,
            imported.Person.Name,
            imported.Person.SocialName,
            imported.LegalNatureCode,
            imported.LegalNatureName,
            imported.IsPrivateSector,
            false,
            null,
            mainAddress is null
                ? null
                : new BrokeragePreviewAddressResponse(
                    mainAddress.ZipCode,
                    mainAddress.Street,
                    mainAddress.Number,
                    mainAddress.Complement,
                    mainAddress.Neighborhood,
                    mainAddress.City,
                    mainAddress.State));
    }

    private static BrokeragePreviewAddressResponse? MapAddress(PersonMainAddressDto? address)
        => address is null
            ? null
            : new BrokeragePreviewAddressResponse(
                address.ZipCode,
                address.Street,
                address.Number,
                address.Complement,
                address.Neighborhood,
                address.City,
                address.State);
}
