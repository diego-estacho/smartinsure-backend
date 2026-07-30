using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.Services.Scopes;

/// <summary>
/// RN-064/RN-068/RN-069/RN-070 — quem administra o Escopo ativo. O Perfil vem do Vínculo do
/// solicitante naquele Escopo, resolvido por chave natural (nome do Perfil fixo), nunca por GUID.
/// </summary>
public sealed class ScopeAuthorization(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IUserBrokerageMembershipRepository brokerageMembershipRepository,
    IUserPolicyHolderMembershipRepository policyHolderMembershipRepository) : IScopeAuthorization
{
    public async Task<ScopeActor> RequireBrokerageAdministratorAsync(
        string externalIdentity,
        Guid? activeBrokerageId,
        CancellationToken cancellationToken)
    {
        var user = await RequireActiveUserAsync(externalIdentity, cancellationToken);

        // RN-069: sem Corretora ativa não há Escopo em que agir — o Usuário precisa escolher.
        if (activeBrokerageId is not { } brokerageId)
        {
            throw new BusinessRuleException(
                "Selecione a corretora ativa antes de executar esta operação.");
        }

        var membership = await brokerageMembershipRepository.GetByUserAndBrokerageAsync(
            user.Id, brokerageId, cancellationToken)
            ?? throw new UnauthorizedException("O usuário não está vinculado a esta corretora.");

        var brokerageAdministrator = await profileRepository.GetBrokerageAdministratorAsync(cancellationToken)
            ?? throw new BusinessRuleException(
                "Perfil Corretor Administrador não disponível na plataforma.");

        if (membership.ProfileId != brokerageAdministrator.Id)
        {
            throw new UnauthorizedException(
                "Somente o Corretor Administrador da corretora ativa executa esta operação.");
        }

        return new ScopeActor(user, brokerageId);
    }

    public async Task<ScopeActor> RequirePolicyHolderAdministratorAsync(
        string externalIdentity,
        Guid? activePolicyHolderId,
        CancellationToken cancellationToken)
    {
        var user = await RequireActiveUserAsync(externalIdentity, cancellationToken);

        if (activePolicyHolderId is not { } policyHolderId)
        {
            throw new BusinessRuleException(
                "Selecione o tomador ativo antes de executar esta operação.");
        }

        var membership = await policyHolderMembershipRepository.GetByUserAndPolicyHolderAsync(
            user.Id, policyHolderId, cancellationToken)
            ?? throw new UnauthorizedException("O usuário não está vinculado a este tomador.");

        var policyHolderAdministrator = await profileRepository.GetByNameAsync(
            ProfileNames.PolicyHolderAdministrator, cancellationToken)
            ?? throw new BusinessRuleException(
                "Perfil Tomador Administrador não disponível na plataforma.");

        if (membership.ProfileId != policyHolderAdministrator.Id)
        {
            throw new UnauthorizedException(
                "Somente o Tomador Administrador do tomador ativo executa esta operação.");
        }

        return new ScopeActor(user, policyHolderId);
    }

    public async Task<AdministeredScope> RequireScopeAdministratorAsync(
        string externalIdentity,
        Guid? activeBrokerageId,
        Guid? activePolicyHolderId,
        CancellationToken cancellationToken)
    {
        // RN-069 antes de RN-070: quando o Usuário administra os dois Escopos ao mesmo tempo, a
        // Corretora ativa é o contexto principal do produto (o Tomador ativo é derivado dela).
        if (activeBrokerageId is not null)
        {
            var brokerageActor = await RequireBrokerageAdministratorAsync(
                externalIdentity, activeBrokerageId, cancellationToken);

            return new AdministeredScope(brokerageActor.User, EProfileScope.Brokerage, brokerageActor.ScopeId);
        }

        if (activePolicyHolderId is not null)
        {
            var policyHolderActor = await RequirePolicyHolderAdministratorAsync(
                externalIdentity, activePolicyHolderId, cancellationToken);

            return new AdministeredScope(
                policyHolderActor.User, EProfileScope.PolicyHolder, policyHolderActor.ScopeId);
        }

        throw new BusinessRuleException(
            "Selecione a corretora ou o tomador ativo antes de executar esta operação.");
    }

    private async Task<Core.Entities.User> RequireActiveUserAsync(
        string externalIdentity,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdentityAsync(externalIdentity, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        // RN-005/RN-076: quem não está Ativo não opera a plataforma.
        if (user.Status != EUserStatus.Active)
        {
            throw new BusinessRuleException("Somente um usuário ativo executa esta operação.");
        }

        return user;
    }
}
