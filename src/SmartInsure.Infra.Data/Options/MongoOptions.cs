using System.ComponentModel.DataAnnotations;

namespace SmartInsure.Infra.Data.Options;

public sealed class MongoOptions
{
    public const string SectionName = "Mongo";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;

    [Required]
    public string Database { get; init; } = string.Empty;

    /// <summary>Retenção do log de integração (ADR-102): dias até o TTL do Mongo expirar o documento.</summary>
    [Range(1, 3650)]
    public int IntegrationLogRetentionDays { get; init; } = 30;
}
