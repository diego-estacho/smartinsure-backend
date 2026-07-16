using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus;

/// <summary>RN-007 — transição Ativa ↔ Inativa; mesma situação é conflito de estado.</summary>
public sealed class ChangeInsurerStatusUseCase(
    IInsurerRepository insurerRepository,
    IUnitOfWork unitOfWork) : IChangeInsurerStatusUseCase
{
    public async Task<ChangeInsurerStatusResponse> ExecuteAsync(
        ChangeInsurerStatusRequest request,
        CancellationToken cancellationToken)
    {
        var insurer = await insurerRepository.GetByIdAsync(request.InsurerId, cancellationToken)
            ?? throw new NotFoundException("Seguradora não encontrada no catálogo.");

        var target = Enum.Parse<EInsurerStatus>(request.Status, ignoreCase: true);

        if (target == EInsurerStatus.Active)
        {
            insurer.Activate();
        }
        else
        {
            insurer.Deactivate();
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return new ChangeInsurerStatusResponse(insurer.Id, insurer.Status.ToString());
    }
}
