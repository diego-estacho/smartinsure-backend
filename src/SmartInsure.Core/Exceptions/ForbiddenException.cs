namespace SmartInsure.Core.Exceptions;

/// <summary>Usuário autenticado sem permissão para a operação — mapeada para 403 pelo resolver central (ADR-012, ADR-022).</summary>
public sealed class ForbiddenException(string message) : Exception(message);
