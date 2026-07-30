using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.GetQuotationGroup.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.GetQuotationGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.GetQuotationGroup.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.GetQuotationGroup;

/// <summary>
/// Lê o Grupo de Cotação por id e resolve o que o wizard precisa para se reidratar ao atualizar a página
/// (RN-050/RN-051): os escalares do pedido, a Cotação escolhida (RN-059) e o Tomador/Segurado/Modalidade
/// já resolvidos. Leitura pura — não muda estado nem cota.
/// </summary>
public sealed class GetQuotationGroupUseCase(
    IQuotationGroupRepository quotationGroupRepository,
    IPersonRepository personRepository,
    IModalityRepository modalityRepository) : IGetQuotationGroupUseCase
{
    public async Task<GetQuotationGroupResponse> ExecuteAsync(
        GetQuotationGroupRequest request, CancellationToken cancellationToken)
    {
        var group = await quotationGroupRepository.GetByIdWithInsurersAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Grupo de cotação não encontrado.");

        var policyHolder = await personRepository.GetSummaryByIdAsync(group.PolicyHolderId, cancellationToken)
            ?? throw new NotFoundException("Tomador não encontrado.");

        var insured = await personRepository.GetSummaryByIdAsync(group.InsuredId, cancellationToken)
            ?? throw new NotFoundException("Segurado não encontrado.");

        var modality = await modalityRepository.GetByIdAsync(group.ModalityId, cancellationToken)
            ?? throw new NotFoundException("Modalidade não encontrada.");

        return new GetQuotationGroupResponse(
            group.Id,
            group.PolicyHolderId,
            group.InsuredId,
            group.ModalityId,
            modality.Name,
            group.InsuredAmount,
            group.CoverageStartDate,
            group.CoverageEndDate,
            group.ScopeMode.ToString(),
            group.SelectedInsurers.Select(insurer => insurer.InsurerId).ToList(),
            group.IncludesPenaltyCoverage,
            group.IncludesLaborCoverage,
            group.Status.ToString(),
            group.SelectedQuotationId,
            MapPerson(policyHolder),
            MapPerson(insured));
    }

    private static QuotationGroupPersonResponse MapPerson(PersonSearchItemDto person)
        => new(
            person.Id,
            person.DocumentNumber,
            person.Name,
            person.SocialName,
            person.MainAddress is null
                ? null
                : new QuotationGroupPersonAddressResponse(
                    person.MainAddress.ZipCode,
                    person.MainAddress.Street,
                    person.MainAddress.Number,
                    person.MainAddress.Complement,
                    person.MainAddress.Neighborhood,
                    person.MainAddress.City,
                    person.MainAddress.State));
}
