using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.CreateModalityGroup.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.CreateModalityGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.CreateModalityGroup.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.CreateModalityGroup;

/// <summary>RN-029 — Criação de Grupo de Modalidade (curadoria): nome único no catálogo.</summary>
public sealed class CreateModalityGroupUseCase(
    IModalityGroupRepository modalityGroupRepository,
    IUnitOfWork unitOfWork) : ICreateModalityGroupUseCase
{
    public async Task<CreateModalityGroupResponse> ExecuteAsync(
        CreateModalityGroupRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        if (await modalityGroupRepository.NameExistsAsync(name, null, cancellationToken))
        {
            throw new ConflictException("Já existe um grupo de modalidade com este nome.");
        }

        if (!Enum.TryParse<EModalityGroupStatus>(request.InitialStatus, ignoreCase: true, out var initialStatus))
        {
            throw new BusinessRuleException("A situação inicial deve ser Active ou Inactive.");
        }

        var group = ModalityGroup.Create(name, request.Description, request.DisplayOrder, initialStatus);

        await modalityGroupRepository.AddAsync(group, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new CreateModalityGroupResponse(
            group.Id, group.Name, group.Description, group.DisplayOrder, group.Status.ToString());
    }
}
