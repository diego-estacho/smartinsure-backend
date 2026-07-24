using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup;

/// <summary>
/// RN-051 — enquanto Rascunho, atualiza o Grupo de Cotação no lugar (mesmo id). Tomador, Segurado e
/// Modalidade precisam existir; o estado não muda aqui.
/// </summary>
public sealed class UpdateQuotationGroupUseCase(
    IQuotationGroupRepository quotationGroupRepository,
    IPersonRepository personRepository,
    IModalityRepository modalityRepository,
    IUnitOfWork unitOfWork) : IUpdateQuotationGroupUseCase
{
    public async Task<UpdateQuotationGroupResponse> ExecuteAsync(
        UpdateQuotationGroupRequest request,
        CancellationToken cancellationToken)
    {
        var scopeMode = ParseScopeMode(request.ScopeMode);

        var group = await quotationGroupRepository.GetByIdWithInsurersAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Grupo de cotação não encontrado.");

        _ = await personRepository.GetByIdAsync(request.PolicyHolderId, cancellationToken)
            ?? throw new NotFoundException("Tomador não encontrado.");

        _ = await personRepository.GetByIdAsync(request.InsuredId, cancellationToken)
            ?? throw new NotFoundException("Segurado não encontrado.");

        _ = await modalityRepository.GetByIdAsync(request.ModalityId, cancellationToken)
            ?? throw new NotFoundException("Modalidade não encontrada.");

        group.UpdateDraft(
            request.PolicyHolderId,
            request.InsuredId,
            request.ModalityId,
            request.InsuredAmount,
            request.CoverageStartDate,
            request.CoverageEndDate,
            scopeMode,
            request.InsurerIds,
            request.IncludesPenaltyCoverage,
            request.IncludesLaborCoverage);

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
            group.IncludesPenaltyCoverage,
            group.IncludesLaborCoverage,
            group.Status.ToString());
    }

    private static EQuotationScopeMode ParseScopeMode(string scopeMode)
        => Enum.TryParse<EQuotationScopeMode>(scopeMode, ignoreCase: true, out var parsed)
            ? parsed
            : throw new BusinessRuleException("O escopo de seguradoras informado é inválido.");
}
