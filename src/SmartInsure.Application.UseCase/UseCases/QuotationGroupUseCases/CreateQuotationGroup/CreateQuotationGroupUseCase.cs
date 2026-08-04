using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup;

/// <summary>
/// RN-050 — cria o Grupo de Cotação em Rascunho ao concluir a etapa de risco. Tomador, Segurado e
/// Modalidade precisam existir; nenhuma Cotação é solicitada aqui (cotar segue fora de escopo — OPEN-07).
/// </summary>
public sealed class CreateQuotationGroupUseCase(
    IQuotationGroupRepository quotationGroupRepository,
    IPersonRepository personRepository,
    IModalityRepository modalityRepository,
    IAdditionalCoverageRepository additionalCoverageRepository,
    IUnitOfWork unitOfWork) : ICreateQuotationGroupUseCase
{
    public async Task<CreateQuotationGroupResponse> ExecuteAsync(
        CreateQuotationGroupRequest request,
        CancellationToken cancellationToken)
    {
        var scopeMode = ParseScopeMode(request.ScopeMode);

        var policyHolder = await personRepository.GetByIdWithRolesAsync(request.PolicyHolderId, cancellationToken)
            ?? throw new NotFoundException("Tomador não encontrado.");

        if (policyHolder.GetRole(EPersonRole.PolicyHolder) is null)
        {
            throw new NotFoundException("Tomador não encontrado.");
        }

        var insured = await personRepository.GetByIdWithRolesAsync(request.InsuredId, cancellationToken)
            ?? throw new NotFoundException("Segurado não encontrado.");

        if (insured.GetRole(EPersonRole.Insured) is null)
        {
            throw new NotFoundException("Segurado não encontrado.");
        }

        _ = await modalityRepository.GetByIdAsync(request.ModalityId, cancellationToken)
            ?? throw new NotFoundException("Modalidade não encontrada.");

        // RN-102: a Filial precisa pertencer à matriz que é o Tomador do grupo; ausente significa a matriz.
        if (request.BranchId is not null)
        {
            var branch = await personRepository.GetTrackedByIdAsync(request.BranchId.Value, cancellationToken)
                ?? throw new NotFoundException("Filial não encontrada.");

            if (branch.HeadquartersPersonId != request.PolicyHolderId)
            {
                throw new BusinessRuleException("A filial informada não pertence ao tomador do grupo de cotação.");
            }
        }

        // RN-104: a escolha é pela Cobertura Adicional canônica — id inexistente é recusado aqui,
        // no mesmo padrão de Tomador/Segurado/Modalidade.
        await EnsureAdditionalCoveragesExistAsync(
            additionalCoverageRepository, request.AdditionalCoverageIds, cancellationToken);

        var group = QuotationGroup.Create(
            request.PolicyHolderId,
            request.BranchId,
            request.InsuredId,
            request.ModalityId,
            request.InsuredAmount,
            request.CoverageStartDate,
            request.CoverageEndDate,
            scopeMode,
            request.InsurerIds,
            request.AdditionalCoverageIds);

        await quotationGroupRepository.AddAsync(group, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new CreateQuotationGroupResponse(
            group.Id,
            group.PolicyHolderId,
            group.InsuredId,
            group.ModalityId,
            group.InsuredAmount,
            group.CoverageStartDate,
            group.CoverageEndDate,
            group.ScopeMode.ToString(),
            group.SelectedInsurers.Select(insurer => insurer.InsurerId).ToList(),
            group.AdditionalCoverages.Select(coverage => coverage.AdditionalCoverageId).ToList(),
            group.Status.ToString());
    }

    /// <summary>RN-104: toda Cobertura Adicional escolhida tem de existir no catálogo canônico.</summary>
    internal static async Task EnsureAdditionalCoveragesExistAsync(
        IAdditionalCoverageRepository repository,
        IReadOnlyList<Guid> additionalCoverageIds,
        CancellationToken cancellationToken)
    {
        foreach (var coverageId in additionalCoverageIds.Distinct())
        {
            _ = await repository.GetByIdAsync(coverageId, cancellationToken)
                ?? throw new NotFoundException("Cobertura adicional não encontrada.");
        }
    }

    private static EQuotationScopeMode ParseScopeMode(string scopeMode)
        => Enum.TryParse<EQuotationScopeMode>(scopeMode, ignoreCase: true, out var parsed)
            ? parsed
            : throw new BusinessRuleException("O escopo de seguradoras informado é inválido.");
}
