using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup;

/// <summary>
/// RN-051/RN-060 — enquanto Rascunho E sem Cotações, atualiza o Grupo de Cotação no lugar (mesmo id).
/// Um Grupo que já tem Cotações é imutável nos dados-base: a edição é recusada (fail-closed) e a
/// mudança segue pela criação de um novo Grupo. Tomador, Segurado e Modalidade precisam existir.
/// </summary>
public sealed class UpdateQuotationGroupUseCase(
    IQuotationGroupRepository quotationGroupRepository,
    IQuotationRepository quotationRepository,
    IPersonRepository personRepository,
    IModalityRepository modalityRepository,
    IAdditionalCoverageRepository additionalCoverageRepository,
    IUnitOfWork unitOfWork) : IUpdateQuotationGroupUseCase
{
    public async Task<UpdateQuotationGroupResponse> ExecuteAsync(
        UpdateQuotationGroupRequest request,
        CancellationToken cancellationToken)
    {
        var scopeMode = ParseScopeMode(request.ScopeMode);

        var group = await quotationGroupRepository.GetByIdWithInsurersAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Grupo de cotação não encontrado.");

        // RN-051: só se atualiza enquanto Rascunho; qualquer outro estado é conflito.
        if (group.Status != EQuotationGroupStatus.Draft)
        {
            throw new ConflictException("O grupo de cotação só pode ser atualizado enquanto está em Rascunho.");
        }

        // RN-060: um Grupo que já tem Cotações é imutável nos dados-base. A alteração não edita este
        // Grupo — segue pela criação de um novo (fork no front); aqui o servidor recusa (fail-closed).
        if (await quotationRepository.ExistsForGroupAsync(request.Id, cancellationToken))
        {
            throw new ConflictException(
                "O grupo de cotação já possui cotações e não pode ter os dados alterados; inicie uma nova cotação.");
        }

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

        // RN-102: a Filial precisa pertencer à matriz que é o Tomador do grupo; ausente limpa o
        // estabelecimento (trocar o Tomador limpa a Filial — sem revalidação, ela some).
        if (request.BranchId is not null)
        {
            var branch = await personRepository.GetTrackedByIdAsync(request.BranchId.Value, cancellationToken)
                ?? throw new NotFoundException("Filial não encontrada.");

            if (branch.HeadquartersPersonId != request.PolicyHolderId)
            {
                throw new BusinessRuleException("A filial informada não pertence ao tomador do grupo de cotação.");
            }
        }

        // RN-104: mesma validação da criação — id de canônica inexistente é recusado.
        await CreateQuotationGroup.CreateQuotationGroupUseCase.EnsureAdditionalCoveragesExistAsync(
            additionalCoverageRepository, request.AdditionalCoverageIds, cancellationToken);

        group.UpdateDraft(
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

        // Sem repository.Update: a raiz e a coleção do escopo estão rastreadas (GetByIdWithInsurersAsync),
        // então o change tracker resolve UPDATE da raiz + INSERT/DELETE dos filhos no commit.
        await unitOfWork.CommitAsync(cancellationToken);

        return new UpdateQuotationGroupResponse(
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

    private static EQuotationScopeMode ParseScopeMode(string scopeMode)
        => Enum.TryParse<EQuotationScopeMode>(scopeMode, ignoreCase: true, out var parsed)
            ? parsed
            : throw new BusinessRuleException("O escopo de seguradoras informado é inválido.");
}
