using Microsoft.Extensions.Caching.Distributed;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserScopeProfile.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserScopeProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserScopeProfile.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserScopeProfile;

/// <summary>
/// RN-075 — troca o Perfil do Usuário dentro de um Escopo (Corretora ou Tomador): substituição
/// direta, nunca deixando o Usuário sem Perfil no Escopo. O novo Perfil precisa ser do mesmo Escopo
/// (RN-072). Invalida o cache de permissões para efeito imediato (como a RN-012).
/// </summary>
public sealed class ChangeUserScopeProfileUseCase(
    IUserRepository userRepository,
    IUserBrokerageMembershipRepository brokerageMembershipRepository,
    IUserPolicyHolderMembershipRepository policyHolderMembershipRepository,
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork,
    IDistributedCache cache) : IChangeUserScopeProfileUseCase
{
    public async Task<ChangeUserScopeProfileResponse> ExecuteAsync(
        ChangeUserScopeProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado na plataforma.");

        var profile = await profileRepository.GetByIdAsync(request.ProfileId, cancellationToken)
            ?? throw new NotFoundException("Perfil não encontrado.");

        var brokerageMembership = await brokerageMembershipRepository.GetByUserAndBrokerageAsync(
            request.UserId, request.ScopeId, cancellationToken);

        if (brokerageMembership is not null)
        {
            EnsureProfileFitsScope(profile, EProfileScope.Brokerage, profile.BrokerageId, request.ScopeId, "corretora");
            brokerageMembership.ChangeProfile(profile.Id);
            brokerageMembershipRepository.Update(brokerageMembership);
        }
        else
        {
            var policyHolderMembership = await policyHolderMembershipRepository.GetByUserAndPolicyHolderAsync(
                request.UserId, request.ScopeId, cancellationToken)
                ?? throw new NotFoundException(
                    "Vínculo do usuário no escopo informado não foi encontrado.");

            EnsureProfileFitsScope(profile, EProfileScope.PolicyHolder, profile.PolicyHolderId, request.ScopeId, "tomador");
            policyHolderMembership.ChangeProfile(profile.Id);
            policyHolderMembershipRepository.Update(policyHolderMembership);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.UserProfile(user.ExternalIdentity), cancellationToken);

        return new ChangeUserScopeProfileResponse(request.UserId, request.ScopeId, profile.Id, profile.Name);
    }

    /// <summary>
    /// RN-075/RN-072: o Perfil precisa ser do Escopo do vínculo — fixo (sem dono) vale para qualquer
    /// Corretora/Tomador; customizado precisa pertencer àquele Escopo específico.
    /// </summary>
    private static void EnsureProfileFitsScope(
        Profile profile, EProfileScope expectedScope, Guid? ownerId, Guid scopeId, string escopo)
    {
        var fits = profile.Scope == expectedScope && (ownerId is null || ownerId == scopeId);

        if (!fits)
        {
            throw new BusinessRuleException($"O novo perfil precisa ser do escopo desta {escopo}.");
        }
    }
}
