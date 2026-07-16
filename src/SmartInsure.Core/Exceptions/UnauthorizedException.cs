namespace SmartInsure.Core.Exceptions;

/// <summary>Falha de autenticação — mapeada para 401 pelo resolver central (ADR-012, ADR-022).</summary>
public sealed class UnauthorizedException(string message) : Exception(message);
