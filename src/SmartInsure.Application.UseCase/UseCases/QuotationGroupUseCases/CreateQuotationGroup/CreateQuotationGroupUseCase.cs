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
    IUnitOfWork unitOfWork) : ICreateQuotationGroupUseCase
{
    public async Task<CreateQuotationGroupResponse> ExecuteAsync(
        CreateQuotationGroupRequest request,
        CancellationToken cancellationToken)
    {
        var scopeMode = ParseScopeMode(request.ScopeMode);

        _ = await personRepository.GetByIdAsync(request.PolicyHolderId, cancellationToken)
            ?? throw new NotFoundException("Tomador não encontrado.");

        _ = await personRepository.GetByIdAsync(request.InsuredId, cancellationToken)
            ?? throw new NotFoundException("Segurado não encontrado.");

        _ = await modalityRepository.GetByIdAsync(request.ModalityId, cancellationToken)
            ?? throw new NotFoundException("Modalidade não encontrada.");

        var group = QuotationGroup.Create(
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
            group.IncludesPenaltyCoverage,
            group.IncludesLaborCoverage,
            group.Status.ToString());
    }

    private static EQuotationScopeMode ParseScopeMode(string scopeMode)
        => Enum.TryParse<EQuotationScopeMode>(scopeMode, ignoreCase: true, out var parsed)
            ? parsed
            : throw new BusinessRuleException("O escopo de seguradoras informado é inválido.");
}
