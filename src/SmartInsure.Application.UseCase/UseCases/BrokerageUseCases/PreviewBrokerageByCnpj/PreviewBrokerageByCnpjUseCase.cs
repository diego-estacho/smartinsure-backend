using SmartInsure.Application.UseCase.Services.PersonImports;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj;

/// <summary>
/// RN-052 — consulta somente leitura de um CNPJ para o cadastro de Corretora: reaproveita os dados já
/// cadastrados ou consulta o Birô, SEM gravar nada (este fluxo não tem <c>UnitOfWork.CommitAsync</c>).
/// Sinaliza quando o CNPJ já possui papel Corretor, com o atalho para o cadastro existente.
/// </summary>
public sealed class PreviewBrokerageByCnpjUseCase(
    IPersonRepository personRepository,
    IPersonBureauImporter personBureauImporter) : IPreviewBrokerageByCnpjUseCase
{
    private const string NotFoundMessage = "CNPJ não localizado na fonte de dados cadastrais.";

    public async Task<BrokeragePreviewResponse> ExecuteAsync(
        PreviewBrokerageByCnpjRequest request,
        CancellationToken cancellationToken)
    {
        var cnpj = CnpjValidator.Normalize(request.Cnpj);

        var existing = await personRepository.FindBrokeragePreviewByDocumentAsync(cnpj, cancellationToken);
        if (existing is not null)
        {
            return new BrokeragePreviewResponse(
                existing.DocumentNumber,
                existing.Name,
                existing.SocialName,
                existing.LegalNatureCode,
                existing.LegalNatureName,
                existing.IsPrivateSector,
                existing.HasBrokerRole,
                existing.HasBrokerRole ? existing.PersonId : null,
                MapAddress(existing.MainAddress));
        }

        // RN-052: importação somente leitura — nada é gravado (sem AddAsync/CommitAsync).
        var imported = await personBureauImporter.ImportLegalPersonAsync(
            cnpj, EPersonRole.Broker, cancellationToken)
            ?? throw new BusinessRuleException(NotFoundMessage);

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
