using System.ComponentModel.DataAnnotations;

namespace SmartInsure.Infra.BackgroundServices.Options;

/// <summary>
/// Travas operacionais do fan-out de cotação (ADR-053, ADR-050): configuráveis com default, sem valor
/// fixo no código. Config de infra (não de negócio) — OPEN-07. Um eventual teto de re-solicitações por
/// Grupo será decidido com dado de uso real.
/// </summary>
public sealed class QuotationFanOutOptions
{
    public const string SectionName = "QuotationFanOut";

    /// <summary>Capacidade da fila in-process (backpressure quando cheia).</summary>
    [Range(1, 100_000)]
    public int ChannelCapacity { get; init; } = 1_000;

    /// <summary>Chamadas simultâneas ao motor (concorrência do fan-out).</summary>
    [Range(1, 128)]
    public int MaxConcurrency { get; init; } = 8;

    /// <summary>Tempo-limite por Seguradora antes de marcar a Cotação como indisponível.</summary>
    [Range(5, 600)]
    public int PerInsurerTimeoutSeconds { get; init; } = 90;

    /// <summary>Cadência do reconciliador (reenfileira Requested presas em restart/deploy).</summary>
    [Range(10, 3_600)]
    public int ReconcilerIntervalSeconds { get; init; } = 120;

    /// <summary>Idade a partir da qual uma Cotação Requested é considerada presa e reenfileirada.</summary>
    [Range(30, 86_400)]
    public int StaleRequestedAfterSeconds { get; init; } = 300;
}
