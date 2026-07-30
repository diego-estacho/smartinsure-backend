namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.DeleteScopedProfile.Requests;

/// <summary>RN-074: remoção de Perfil customizado do próprio Escopo (bloqueada se estiver em uso).</summary>
public sealed record DeleteScopedProfileRequest(
    string ExternalIdentity,
    Guid? ActiveBrokerageId,
    Guid? ActivePolicyHolderId,
    Guid ProfileId);
