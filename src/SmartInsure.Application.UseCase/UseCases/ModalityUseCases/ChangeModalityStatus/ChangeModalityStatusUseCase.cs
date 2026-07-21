using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus;

/// <summary>RN-036 — transição Ativa ↔ Inativa da Modalidade; mesma situação é conflito de estado.</summary>
public sealed class ChangeModalityStatusUseCase(
    IModalityRepository modalityRepository,
    IUnitOfWork unitOfWork) : IChangeModalityStatusUseCase
{
    public async Task<ChangeModalityStatusResponse> ExecuteAsync(
        ChangeModalityStatusRequest request,
        CancellationToken cancellationToken)
    {
        var modality = await modalityRepository.GetByIdAsync(request.ModalityId, cancellationToken)
            ?? throw new NotFoundException("Modalidade não encontrada no catálogo.");

        if (!Enum.TryParse<EModalityStatus>(request.Status, ignoreCase: true, out var target))
        {
            throw new BusinessRuleException("A situação deve ser Active ou Inactive.");
        }

        if (target == EModalityStatus.Active)
        {
            modality.Activate();
        }
        else
        {
            modality.Deactivate();
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return new ChangeModalityStatusResponse(modality.Id, modality.Status.ToString());
    }
}
