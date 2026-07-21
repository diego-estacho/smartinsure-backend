using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.GetModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.GetModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.GetModality.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.GetModality;

/// <summary>Detalhe de Modalidade do catálogo (leitura, RN-029).</summary>
public sealed class GetModalityUseCase(IModalityRepository modalityRepository) : IGetModalityUseCase
{
    public async Task<GetModalityResponse> ExecuteAsync(
        GetModalityRequest request,
        CancellationToken cancellationToken)
    {
        var modality = await modalityRepository.GetByIdAsync(request.ModalityId, cancellationToken)
            ?? throw new NotFoundException("Modalidade não encontrada no catálogo.");

        return new GetModalityResponse(
            modality.Id, modality.Name, modality.ModalityGroupId, modality.Description, modality.Status.ToString());
    }
}
