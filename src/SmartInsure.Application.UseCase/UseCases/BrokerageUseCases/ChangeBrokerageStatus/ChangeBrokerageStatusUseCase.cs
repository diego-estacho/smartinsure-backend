using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus;

/// <summary>RN-021 — ativa/inativa Corretora alterando a situação do papel Corretor.</summary>
public sealed class ChangeBrokerageStatusUseCase(
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork) : IChangeBrokerageStatusUseCase
{
    public async Task<ChangeBrokerageStatusResponse> ExecuteAsync(
        ChangeBrokerageStatusRequest request,
        CancellationToken cancellationToken)
    {
        var person = await personRepository.GetTrackedBrokerageByIdAsync(
            request.BrokerageId, cancellationToken)
            ?? throw new NotFoundException("Corretora não encontrada.");

        var role = person.GetRole(EPersonRole.Broker)
            ?? throw new NotFoundException("Corretora não encontrada.");

        if (!Enum.TryParse<EPersonRoleStatus>(request.Status, ignoreCase: true, out var target))
        {
            throw new BusinessRuleException("A situação deve ser Active ou Inactive.");
        }

        if (target == EPersonRoleStatus.Active)
        {
            role.Activate();
        }
        else
        {
            role.Deactivate();
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return new ChangeBrokerageStatusResponse(person.Id, role.Status.ToString());
    }
}
