using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ChangeBrokerageInsurerEnablementStatus.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ChangeBrokerageInsurerEnablementStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ChangeBrokerageInsurerEnablementStatus.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ChangeBrokerageInsurerEnablementStatus;

/// <summary>RN-022 — Ativa ↔ Inativa; a Habilitação nunca é excluída.</summary>
public sealed class ChangeBrokerageInsurerEnablementStatusUseCase(
    IBrokerageInsurerEnablementRepository enablementRepository,
    IUnitOfWork unitOfWork) : IChangeBrokerageInsurerEnablementStatusUseCase
{
    public async Task<ChangeBrokerageInsurerEnablementStatusResponse> ExecuteAsync(
        ChangeBrokerageInsurerEnablementStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<EBrokerageInsurerEnablementStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new BusinessRuleException("A situação deve ser Active ou Inactive.");
        }

        var enablement = await enablementRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Habilitação não encontrada.");

        if (status == EBrokerageInsurerEnablementStatus.Active)
        {
            enablement.Activate();
        }
        else
        {
            enablement.Deactivate();
        }

        enablementRepository.Update(enablement);
        await unitOfWork.CommitAsync(cancellationToken);

        return new ChangeBrokerageInsurerEnablementStatusResponse(enablement.Id, enablement.Status.ToString());
    }
}
