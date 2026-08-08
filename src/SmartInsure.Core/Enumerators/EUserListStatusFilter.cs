namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Filtro de situação da listagem de Usuários — vocabulário da tela, não a situação de domínio
/// (<see cref="EUserStatus"/>). "Expirado" e "Pendente (não expirado)" são recortes de
/// <c>Pending</c> pela validade do Convite (RN-065); o item continua carregando a situação estável
/// do enum + o flag de Convite vencido (ADR-031/ADR-004).
/// </summary>
public enum EUserListStatusFilter
{
    Active,
    Inactive,
    PendingNotExpired,
    Expired,
}
