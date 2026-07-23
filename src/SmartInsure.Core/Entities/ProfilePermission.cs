namespace SmartInsure.Core.Entities;

/// <summary>
/// Vínculo entre um Perfil e uma Permissão (RN-032/RN-033). Uma Permissão do catálogo
/// aparece no Perfil quando marcada.
/// </summary>
public sealed class ProfilePermission : EntityBase
{
    private ProfilePermission()
    {
    }

    public Guid ProfileId { get; private set; }

    public Guid PermissionId { get; private set; }

    public static ProfilePermission Create(Guid profileId, Guid permissionId)
        => new()
        {
            ProfileId = profileId,
            PermissionId = permissionId,
        };
}
