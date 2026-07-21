using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.UpdateModalityGroup.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.UpdateModalityGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.UpdateModalityGroup.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.UpdateModalityGroup;

/// <summary>RN-029 — Edição de Grupo de Modalidade: nome permanece único; situação intacta.</summary>
public sealed class UpdateModalityGroupUseCase(
    IModalityGroupRepository modalityGroupRepository,
    IUnitOfWork unitOfWork) : IUpdateModalityGroupUseCase
{
    public async Task<UpdateModalityGroupResponse> ExecuteAsync(
        UpdateModalityGroupRequest request,
        CancellationToken cancellationToken)
    {
        var group = await modalityGroupRepository.GetByIdAsync(request.ModalityGroupId, cancellationToken)
            ?? throw new NotFoundException("Grupo de modalidade não encontrado no catálogo.");

        var name = request.Name.Trim();

        if (await modalityGroupRepository.NameExistsAsync(name, group.Id, cancellationToken))
        {
            throw new ConflictException("Já existe um grupo de modalidade com este nome.");
        }

        group.Update(name, request.Description, request.DisplayOrder);
        await unitOfWork.CommitAsync(cancellationToken);

        return new UpdateModalityGroupResponse(
            group.Id, group.Name, group.Description, group.DisplayOrder, group.Status.ToString());
    }
}
