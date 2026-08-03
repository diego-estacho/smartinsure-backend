namespace SmartInsure.Core.Abstractions.Services;

/// <summary>
/// Registra no Mongo (QuotationIntegrationLog, ADR-102) UMA solicitação de Cotação ao PlugV2
/// (POST /Cotation): request/response, veredito, duração e erro — primeiro consumidor real do
/// IMongoRepository&lt;&gt;. Best-effort por contrato: a implementação NUNCA deve propagar falha de
/// gravação — a Cotação não pode depender do log (ADR-102). O recorder calcula internamente
/// CreatedAtUtc/ExpiresAtUtc (retenção), CorrelationId (Activity corrente) e o truncamento dos payloads.
/// </summary>
public interface IQuotationIntegrationLogRecorder
{
    Task RecordCotationAsync(QuotationIntegrationLogContext context, CancellationToken cancellationToken);
}

/// <summary>Veredito da chamada de integração (ADR-102) — não confundir com o resultado de negócio da Cotação (EQuotationResult).</summary>
public static class QuotationIntegrationOutcome
{
    public const string Completed = "Completed";

    public const string Failed = "Failed";
}

/// <summary>
/// Dados de UMA solicitação de Cotação ao PlugV2 para registro no log de integração (ADR-102). Só o corpo
/// (request/response) chega aqui — a PlugKey trafega no header e nunca é logada.
/// </summary>
public sealed record QuotationIntegrationLogContext
{
    public required Guid QuotationId { get; init; }

    public required Guid QuotationGroupId { get; init; }

    public required Guid InsurerId { get; init; }

    /// <summary>Motor de Cálculo que atendeu a solicitação (ex.: "PlugV2").</summary>
    public required string EngineType { get; init; }

    /// <summary>"Completed" ou "Failed" — ver <see cref="QuotationIntegrationOutcome"/>.</summary>
    public required string Outcome { get; init; }

    /// <summary>Resultado de negócio da Cotação (EQuotationResult, como texto) quando Outcome = Completed.</summary>
    public string? QuotationStatus { get; init; }

    public required long DurationMs { get; init; }

    public required string RequestPayload { get; init; }

    public string? ResponseRaw { get; init; }

    public int? HttpStatus { get; init; }

    /// <summary>Texto de erro (do gateway ou da exceção) quando Outcome = Failed.</summary>
    public string? ErrorMessage { get; init; }
}
