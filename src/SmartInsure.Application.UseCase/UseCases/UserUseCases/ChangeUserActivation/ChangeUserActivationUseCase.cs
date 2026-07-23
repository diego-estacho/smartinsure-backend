using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation;

/// <summary>
/// RN-046 — inativa/reativa um Usuário. Nesta fatia a operação é do Administrador do Sistema
/// (autorização no endpoint); a ação por escopo (Corretor/Tomador Administrador, usuário comum
/// com permissão) fica para a fatia dependente de escopo/enforcement ([OPEN-12]). Usuário Inativo
/// não acessa a plataforma (RN-005/RN-046).
/// </summary>
public sealed class ChangeUserActivationUseCase(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork) : IChangeUserActivationUseCase
{
    public async Task<ChangeUserActivationResponse> ExecuteAsync(
        ChangeUserActivationRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado na plataforma.");

        if (request.Activate)
        {
            user.Reactivate();
        }
        else
        {
            // RN-046: a inativação não pode deixar a plataforma sem Administrador do Sistema.
            var systemAdministrator = await profileRepository.GetSystemAdministratorAsync(cancellationToken);

            if (systemAdministrator is not null
                && user.ProfileId == systemAdministrator.Id
                && await userRepository.CountByProfileIdAsync(systemAdministrator.Id, cancellationToken) <= 1)
            {
                throw new BusinessRuleException(
                    "A plataforma não pode ficar sem Administrador do Sistema.");
            }

            user.Deactivate();
        }

        userRepository.Update(user);
        await unitOfWork.CommitAsync(cancellationToken);

        return new ChangeUserActivationResponse(user.Id, user.Status.ToString());
    }
}
