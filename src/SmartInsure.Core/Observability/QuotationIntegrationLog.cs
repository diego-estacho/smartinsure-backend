namespace SmartInsure.Core.Observability;

/// <summary>
/// Documento Mongo (ADR-102): registro de UMA solicitação de Cotação ao PlugV2 (POST /Cotation) —
/// request/response (só o corpo; a PlugKey trafega no header <c>application-key-v2</c> e nunca é logada,
/// SECURITY.md), veredito, duração e erro. Collection resolvida por convenção em
/// <see cref="Repositories.IMongoRepository{TDocument}"/> (ADR-039): <c>nameof(QuotationIntegrationLog)</c> =
/// "QuotationIntegrationLog". Expira por TTL em <see cref="ExpiresAtUtc"/> — índice garantido no startup da
/// API (ADR-102), nunca no domínio.
/// </summary>
public sealed class QuotationIntegrationLog
{
    public Guid QuotationId { get; init; }

    public Guid QuotationGroupId { get; init; }

    public Guid InsurerId { get; init; }

    /// <summary>Motor de Cálculo que atendeu a solicitação (ex.: "PlugV2").</summary>
    public required string EngineType { get; init; }

    /// <summary>Veredito da chamada de integração: "Completed" ou "Failed" (ver <see cref="Abstractions.Services.QuotationIntegrationOutcome"/>) — não confundir com o resultado de negócio da Cotação.</summary>
    public required string Outcome { get; init; }

    /// <summary>Resultado de negócio da Cotação (EQuotationResult, como texto) quando Outcome = Completed.</summary>
    public string? QuotationStatus { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Retenção via TTL (ADR-102): CreatedAtUtc + Mongo:IntegrationLogRetentionDays.</summary>
    public DateTime ExpiresAtUtc { get; init; }

    public long DurationMs { get; init; }

    /// <summary>Id do Activity corrente (W3C) no momento da gravação — liga com App Insights/OpenTelemetry.</summary>
    public string? CorrelationId { get; init; }

    public required QuotationIntegrationLogPayload Request { get; init; }

    public required QuotationIntegrationLogResponse Response { get; init; }

    /// <summary>Texto de erro (do gateway ou da exceção) quando Outcome = Failed.</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>Corpo enviado ao gateway PlugV2 (ADR-102) — nunca inclui headers/segredo, só o payload de negócio.</summary>
public sealed record QuotationIntegrationLogPayload
{
    public required string Payload { get; init; }

    /// <summary>Verdadeiro quando o payload excedeu 256 KB e foi truncado antes de gravar.</summary>
    public bool Truncated { get; init; }
}

/// <summary>Corpo recebido do gateway PlugV2 (ADR-102), com o status HTTP quando disponível.</summary>
public sealed record QuotationIntegrationLogResponse
{
    public required string Raw { get; init; }

    public int? HttpStatus { get; init; }

    /// <summary>Verdadeiro quando o payload excedeu 256 KB e foi truncado antes de gravar.</summary>
    public bool Truncated { get; init; }
}
