namespace SmartInsure.Core.Exceptions;

/// <summary>
/// Exceção lançada quando o motor de cálculo falha (RN-030: falha isolada na consulta).
/// Nunca deve ser tratada como exceção de negócio — é interna da integração e sempre
/// resulta em resultado Unavailable no contexto da Consulta de Crédito.
/// </summary>
public sealed class CalculationEngineException : Exception
{
    public CalculationEngineException(string message)
        : base(message)
    {
    }

    public CalculationEngineException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
