namespace SmartInsure.Application.UseCase.Common;

/// <summary>Retorno de UseCase sem payload (ADR-020).</summary>
public readonly record struct Unit
{
    public static readonly Unit Value = new();
}
