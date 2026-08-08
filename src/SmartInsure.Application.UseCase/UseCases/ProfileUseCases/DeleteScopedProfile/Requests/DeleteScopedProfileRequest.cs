namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.DeleteScopedProfile.Requests;

/// <summary>
/// RN-074: remoção de Perfil customizado do próprio Escopo. Se estiver em uso, exige
/// <see cref="MigrateToProfileId"/> — o Perfil de destino (mesmo Escopo) para onde os Usuários
/// migram antes da remoção (RN-075).
/// </summary>
public sealed record DeleteScopedProfileRequest(
    string ExternalIdentity,
    Guid? ActiveBrokerageId,
    Guid? ActivePolicyHolderId,
    Guid ProfileId,
    Guid? MigrateToProfileId = null);
