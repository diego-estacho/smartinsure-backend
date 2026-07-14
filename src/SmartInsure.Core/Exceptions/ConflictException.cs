namespace SmartInsure.Core.Exceptions;

/// <summary>Conflito de estado — mapeada para 409 pelo resolver central (ADR-012, ADR-022).</summary>
public sealed class ConflictException(string message) : Exception(message);
