using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality;

/// <summary>RN-032 — Edição de Modalidade (nome/descrição): nome permanece único; situação e id global intactos.</summary>
public sealed class UpdateModalityUseCase(
    IModalityRepository modalityRepository,
    IUnitOfWork unitOfWork) : IUpdateModalityUseCase
{
    public async Task<UpdateModalityResponse> ExecuteAsync(
        UpdateModalityRequest request,
        CancellationToken cancellationToken)
    {
        var modality = await modalityRepository.GetByIdAsync(request.ModalityId, cancellationToken)
            ?? throw new NotFoundException("Modalidade não encontrada no catálogo.");

        var name = request.Name.Trim();

        if (await modalityRepository.NameExistsAsync(name, modality.Id, cancellationToken))
        {
            throw new ConflictException("Já existe uma modalidade com este nome no catálogo.");
        }

        modality.Update(name, request.Description);
        await unitOfWork.CommitAsync(cancellationToken);

        return new UpdateModalityResponse(
            modality.Id, modality.Name, modality.Description, modality.Status.ToString());
    }
}
