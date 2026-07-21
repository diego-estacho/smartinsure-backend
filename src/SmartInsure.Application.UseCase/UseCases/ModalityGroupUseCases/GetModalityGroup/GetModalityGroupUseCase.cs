using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.GetModalityGroup.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.GetModalityGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.GetModalityGroup.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.GetModalityGroup;

/// <summary>Detalhe de Grupo de Modalidade do catálogo (leitura, RN-029).</summary>
public sealed class GetModalityGroupUseCase(IModalityGroupRepository modalityGroupRepository)
    : IGetModalityGroupUseCase
{
    public async Task<GetModalityGroupResponse> ExecuteAsync(
        GetModalityGroupRequest request,
        CancellationToken cancellationToken)
    {
        var group = await modalityGroupRepository.GetByIdAsync(request.ModalityGroupId, cancellationToken)
            ?? throw new NotFoundException("Grupo de modalidade não encontrado no catálogo.");

        return new GetModalityGroupResponse(
            group.Id, group.Name, group.Description, group.DisplayOrder, group.Status.ToString());
    }
}
