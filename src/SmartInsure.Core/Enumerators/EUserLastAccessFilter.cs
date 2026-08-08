namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Filtro de último acesso da listagem de Usuários (RN-204) — recorte da tela, não domínio.
/// "Nunca" = sem acesso registrado; os demais são "acessou nos últimos N dias".
/// </summary>
public enum EUserLastAccessFilter
{
    Within7,
    Within30,
    Within90,
    Never,
}
