using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ChangeInsurerEnablementStatus.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ChangeInsurerEnablementStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ChangeInsurerEnablementStatus.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ChangeInsurerEnablementStatus;

/// <summary>RN-022 — Ativa ↔ Inativa; a Habilitação nunca é excluída.</summary>
public sealed class ChangeInsurerEnablementStatusUseCase(
    IInsurerEnablementRepository enablementRepository,
    IUnitOfWork unitOfWork) : IChangeInsurerEnablementStatusUseCase
{
    public async Task<ChangeInsurerEnablementStatusResponse> ExecuteAsync(
        ChangeInsurerEnablementStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<EInsurerEnablementStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new BusinessRuleException("A situação deve ser Active ou Inactive.");
        }

        var enablement = await enablementRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Habilitação não encontrada.");

        if (status == EInsurerEnablementStatus.Active)
        {
            enablement.Activate();
        }
        else
        {
            enablement.Deactivate();
        }

        enablementRepository.Update(enablement);
        await unitOfWork.CommitAsync(cancellationToken);

        return new ChangeInsurerEnablementStatusResponse(enablement.Id, enablement.Status.ToString());
    }
}
