using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Situação de uma Cobertura Adicional escolhida dentro de UMA Cotação (RN-105/RN-106): enviada —
/// com o nome da Importada que foi à Seguradora — ou não contemplada. Gravada em toda Cotação,
/// inclusive nas que resultam Indisponível (RN-058) e nas que falham na integração (RN-057).
/// </summary>
public sealed class QuotationAdditionalCoverage : EntityBase
{
    private QuotationAdditionalCoverage()
    {
    }

    public Guid QuotationId { get; private set; }

    public Guid AdditionalCoverageId { get; private set; }

    public EQuotationAdditionalCoverageStatus Status { get; private set; }

    /// <summary>Nome da Cobertura Adicional Importada enviado à Seguradora; nulo quando não contemplada.</summary>
    public string? SentName { get; private set; }

    /// <summary>
    /// Importada de origem, quando identificável — nulo quando os ramos da Seguradora compartilham o
    /// mesmo nome (o nome enviado continua inequívoco; a linha de origem, não).
    /// </summary>
    public Guid? ImportedAdditionalCoverageId { get; private set; }

    public static QuotationAdditionalCoverage Create(Guid quotationId, ResolvedAdditionalCoverage resolved)
        => new()
        {
            QuotationId = quotationId,
            AdditionalCoverageId = resolved.AdditionalCoverageId,
            Status = resolved.Status,
            SentName = resolved.SentName,
            ImportedAdditionalCoverageId = resolved.ImportedAdditionalCoverageId,
        };
}
