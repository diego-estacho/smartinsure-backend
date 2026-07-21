using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Resultado individual de uma Consulta de Crédito para uma Seguradora (RN-029/RN-030).
/// Status Available: limites agrupados por grupo de modalidade. Status Unavailable: apenas motivo.
/// Imutável após criação — nunca editado ou excluído (RN-031).
/// </summary>
public sealed class CreditInquiryResult : EntityBase
{
    private readonly List<CreditInquiryResultLimit> _limits = [];

    private CreditInquiryResult()
    {
    }

    public Guid CreditInquiryId { get; private set; }

    public Guid InsurerId { get; private set; }

    public ECreditInquiryResultStatus Status { get; private set; }

    public string? FailureReason { get; private set; }

    /// <summary>Coleção de limites agrupados por grupo de modalidade (apenas quando Status = Available).</summary>
    public IReadOnlyCollection<CreditInquiryResultLimit> Limits => _limits.AsReadOnly();

    /// <summary>RN-030: resultado disponível com limites agrupados por grupo de modalidade.</summary>
    public static CreditInquiryResult Available(
        Guid creditInquiryId,
        Guid insurerId,
        IEnumerable<CreditInquiryResultLimit> limits)
    {
        var result = new CreditInquiryResult
        {
            CreditInquiryId = creditInquiryId,
            InsurerId = insurerId,
            Status = ECreditInquiryResultStatus.Available,
        };

        foreach (var limit in limits)
        {
            result.AddLimit(limit);
        }

        return result;
    }

    /// <summary>RN-030: resultado indisponível com motivo da falha.</summary>
    public static CreditInquiryResult Unavailable(
        Guid creditInquiryId,
        Guid insurerId,
        string failureReason)
        => new()
        {
            CreditInquiryId = creditInquiryId,
            InsurerId = insurerId,
            Status = ECreditInquiryResultStatus.Unavailable,
            FailureReason = failureReason,
        };

    /// <summary>RN-029: adiciona limite ao resultado, vinculando-o ao agregado.</summary>
    public void AddLimit(CreditInquiryResultLimit limit)
    {
        limit.AttachTo(Id);
        _limits.Add(limit);
    }
}
