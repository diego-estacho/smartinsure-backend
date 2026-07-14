namespace SmartInsure.Core.Exceptions;

/// <summary>Recurso inexistente — mapeada para 404 pelo resolver central (ADR-012, ADR-022).</summary>
public sealed class NotFoundException(string message) : Exception(message);
