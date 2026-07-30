namespace SmartInsure.Core.Constants;

/// <summary>Nomes dos Perfis fixos da plataforma — chave natural do Perfil (RN-012, RN-062).</summary>
public static class ProfileNames
{
    public const string SystemAdministrator = nameof(SystemAdministrator);

    public const string BrokerageAdministrator = nameof(BrokerageAdministrator);

    public const string PolicyHolderAdministrator = nameof(PolicyHolderAdministrator);

    /// <summary>
    /// Perfil fixo Corretor (RN-062). Nome técnico decidido em 2026-07-29 (OPEN-17): `BrokerageUser`
    /// para não colidir com o Papel da Pessoa `Broker`, que é conceito distinto.
    /// </summary>
    public const string BrokerageUser = nameof(BrokerageUser);

    /// <summary>
    /// Perfil fixo Tomador (RN-062). Nome técnico decidido em 2026-07-29 (OPEN-17): `PolicyHolderUser`
    /// para não colidir com o Papel da Pessoa `PolicyHolder`.
    /// </summary>
    public const string PolicyHolderUser = nameof(PolicyHolderUser);

    /// <summary>Os cinco Perfis fixos da plataforma — nenhum deles é criado ou removido por tela.</summary>
    public static readonly string[] Fixed =
    [
        SystemAdministrator,
        BrokerageAdministrator,
        PolicyHolderAdministrator,
        BrokerageUser,
        PolicyHolderUser,
    ];

    /// <summary>
    /// Perfis fixos de administração: invisíveis na gestão de Perfis para quem não é
    /// Administrador do Sistema (RN-072) — continuam atribuíveis pela hierarquia (RN-068/069/070).
    /// </summary>
    public static readonly string[] AdministrativeFixed =
    [
        SystemAdministrator,
        BrokerageAdministrator,
        PolicyHolderAdministrator,
    ];
}
