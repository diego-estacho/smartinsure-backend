namespace SmartInsure.Core.Exceptions;

/// <summary>Regra de negócio impede a operação — mapeada para 422 pelo resolver central (ADR-012, ADR-022).</summary>
public sealed class BusinessRuleException(string message) : Exception(message);
