using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ChangeModalityGroupStatus.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ChangeModalityGroupStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ChangeModalityGroupStatus.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ChangeModalityGroupStatus;

/// <summary>RN-036 — transição Ativo ↔ Inativo do Grupo; mesma situação é conflito de estado.</summary>
public sealed class ChangeModalityGroupStatusUseCase(
    IModalityGroupRepository modalityGroupRepository,
    IUnitOfWork unitOfWork) : IChangeModalityGroupStatusUseCase
{
    public async Task<ChangeModalityGroupStatusResponse> ExecuteAsync(
        ChangeModalityGroupStatusRequest request,
        CancellationToken cancellationToken)
    {
        var group = await modalityGroupRepository.GetByIdAsync(request.ModalityGroupId, cancellationToken)
            ?? throw new NotFoundException("Grupo de modalidade não encontrado no catálogo.");

        if (!Enum.TryParse<EModalityGroupStatus>(request.Status, ignoreCase: true, out var target))
        {
            throw new BusinessRuleException("A situação deve ser Active ou Inactive.");
        }

        if (target == EModalityGroupStatus.Active)
        {
            group.Activate();
        }
        else
        {
            group.Deactivate();
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return new ChangeModalityGroupStatusResponse(group.Id, group.Status.ToString());
    }
}
