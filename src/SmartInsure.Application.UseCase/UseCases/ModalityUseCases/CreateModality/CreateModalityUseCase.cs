using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality;

/// <summary>RN-029 — Criação manual de Modalidade (curadoria): nome único no catálogo.</summary>
public sealed class CreateModalityUseCase(
    IModalityRepository modalityRepository,
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

        if (!Enum.TryParse<EModalityStatus>(request.InitialStatus, ignoreCase: true, out var initialStatus))
        {
            throw new BusinessRuleException("A situação inicial deve ser Active ou Inactive.");
        }

        var modality = Modality.CreateManual(name, request.Description, initialStatus);

        await modalityRepository.AddAsync(modality, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new CreateModalityResponse(
            modality.Id, modality.Name, modality.Description, modality.Status.ToString());
    }
}
