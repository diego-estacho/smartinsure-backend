using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality;

/// <summary>RN-029 — Criação de Modalidade (curadoria humana): nome único e Grupo existente.</summary>
public sealed class CreateModalityUseCase(
    IModalityRepository modalityRepository,
    IModalityGroupRepository modalityGroupRepository,
    IUnitOfWork unitOfWork) : ICreateModalityUseCase
{
    public async Task<CreateModalityResponse> ExecuteAsync(
        CreateModalityRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        if (await modalityRepository.NameExistsAsync(name, null, cancellationToken))
        {
            throw new ConflictException("Já existe uma modalidade com este nome no catálogo.");
        }

        _ = await modalityGroupRepository.GetByIdAsync(request.ModalityGroupId, cancellationToken)
            ?? throw new NotFoundException("Grupo de modalidade não encontrado no catálogo.");

        if (!Enum.TryParse<EModalityStatus>(request.InitialStatus, ignoreCase: true, out var initialStatus))
        {
            throw new BusinessRuleException("A situação inicial deve ser Active ou Inactive.");
        }

        var modality = Modality.Create(name, request.ModalityGroupId, request.Description, initialStatus);

        await modalityRepository.AddAsync(modality, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new CreateModalityResponse(
            modality.Id, modality.Name, modality.ModalityGroupId, modality.Description, modality.Status.ToString());
    }
}
