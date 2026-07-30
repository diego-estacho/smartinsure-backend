using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListAssignableProfiles.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListAssignableProfiles.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListAssignableProfiles.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListAssignableProfiles;

/// <summary>
/// RN-072 — Perfis oferecidos na criação de Usuário, pela hierarquia de quem cria:
/// Corretor Administrador oferece Tomador Administrador (RN-068), Corretor e os customizados da
/// Corretora ativa (RN-069); Tomador Administrador oferece Tomador e os customizados do Tomador
/// ativo (RN-070). Corretor Administrador nunca aparece aqui — quem o concede é o Administrador
/// do Sistema (RN-066). Usuário comum depende da Permissão de criar Usuário (RN-071, adiada):
/// devolve lista vazia em vez de oferecer o que ele não pode.
/// </summary>
public sealed class ListAssignableProfilesUseCase(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IUserBrokerageMembershipRepository brokerageMembershipRepository,
    IUserPolicyHolderMembershipRepository policyHolderMembershipRepository)
    : IListAssignableProfilesUseCase
{
    public async Task<IReadOnlyList<AssignableProfileResponse>> ExecuteAsync(
        ListAssignableProfilesRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdentityAsync(
            request.ExternalIdentity, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        var assignable = new List<Profile>();

        // Administrador do Sistema: o fluxo dele é o convite de Corretor Administrador (RN-066).
        if (user.Profile?.Name == ProfileNames.SystemAdministrator)
        {
            assignable.AddRange(await profileRepository.ListByScopeAsync(
                EProfileScope.Brokerage, null, cancellationToken));

            return Map(assignable);
        }

        if (request.ActiveBrokerageId is { } brokerageId
            && await IsBrokerageAdministratorAsync(user.Id, brokerageId, cancellationToken))
        {
            var brokerageProfiles = await profileRepository.ListByScopeAsync(
                EProfileScope.Brokerage, brokerageId, cancellationToken);

            // RN-069: o Perfil Corretor Administrador não é atribuível por outro CA.
            assignable.AddRange(brokerageProfiles.Where(profile =>
                !(profile.IsFixed && profile.Name == ProfileNames.BrokerageAdministrator)));

            // RN-068: o CA também cria Tomador Administrador (o Tomador nomeado é escolhido no fluxo).
            var policyHolderAdministrator = await profileRepository.GetByNameAsync(
                ProfileNames.PolicyHolderAdministrator, cancellationToken);

            if (policyHolderAdministrator is not null)
            {
                assignable.Add(policyHolderAdministrator);
            }
        }

        if (request.ActivePolicyHolderId is { } policyHolderId
            && await IsPolicyHolderAdministratorAsync(user.Id, policyHolderId, cancellationToken))
        {
            var policyHolderProfiles = await profileRepository.ListByScopeAsync(
                EProfileScope.PolicyHolder, policyHolderId, cancellationToken);

            // RN-070: o TA oferece Tomador e os customizados do seu Tomador — não o próprio TA.
            assignable.AddRange(policyHolderProfiles.Where(profile =>
                !(profile.IsFixed && profile.Name == ProfileNames.PolicyHolderAdministrator)));
        }

        return Map(assignable);
    }

    private async Task<bool> IsBrokerageAdministratorAsync(
        Guid userId, Guid brokerageId, CancellationToken cancellationToken)
    {
        var membership = await brokerageMembershipRepository.GetByUserAndBrokerageAsync(
            userId, brokerageId, cancellationToken);

        if (membership is null)
        {
            return false;
        }

        var brokerageAdministrator = await profileRepository.GetBrokerageAdministratorAsync(
            cancellationToken);

        return brokerageAdministrator is not null && membership.ProfileId == brokerageAdministrator.Id;
    }

    private async Task<bool> IsPolicyHolderAdministratorAsync(
        Guid userId, Guid policyHolderId, CancellationToken cancellationToken)
    {
        var membership = await policyHolderMembershipRepository.GetByUserAndPolicyHolderAsync(
            userId, policyHolderId, cancellationToken);

        if (membership is null)
        {
            return false;
        }

        var policyHolderAdministrator = await profileRepository.GetByNameAsync(
            ProfileNames.PolicyHolderAdministrator, cancellationToken);

        return policyHolderAdministrator is not null
            && membership.ProfileId == policyHolderAdministrator.Id;
    }

    private static List<AssignableProfileResponse> Map(IEnumerable<Profile> profiles)
        => profiles
            .DistinctBy(profile => profile.Id)
            .Select(profile => new AssignableProfileResponse(
                profile.Id,
                profile.Name,
                profile.Scope.ToString(),
                profile.IsFixed,
                profile.BrokerageId,
                profile.PolicyHolderId))
            .ToList();
}
