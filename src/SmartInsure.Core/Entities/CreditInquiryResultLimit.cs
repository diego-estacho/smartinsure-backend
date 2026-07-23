namespace SmartInsure.Core.Entities;

/// <summary>
/// Limite de crédito agrupado por grupo de modalidade (RN-029).
/// Um registro por grupo informado pela Seguradora.
/// Valor do grupo = maior limite entre as modalidades que o compõem.
/// Imutável após criação — nunca editado ou excluído (RN-031).
/// </summary>
public sealed class CreditInquiryResultLimit : EntityBase
{
    private CreditInquiryResultLimit()
    {
    }

    public Guid CreditInquiryResultId { get; private set; }

    /// <summary>Nome do grupo de modalidade (ex.: "Tradicional", "Judiciais", "Financeira").</summary>
    public string GroupName { get; private set; } = string.Empty;

    /// <summary>Tipo de grupo (ex.: "GARANTIA_TRADICIONAL", "GARANTIA_JUDICIAL").</summary>
    public string GroupType { get; private set; } = string.Empty;

    /// <summary>Limite disponível — maior AvailableLimit entre modalidades do grupo.</summary>
    public decimal AvailableLimit { get; private set; }

    /// <summary>Limite revisado — maior LimitRevised entre modalidades do grupo.</summary>
    public decimal RevisedLimit { get; private set; }

    /// <summary>Taxa — da modalidade com maior AvailableLimit do grupo.</summary>
    public decimal Rate { get; private set; }

    /// <summary>RN-029: cria limite de crédito para um grupo de modalidade. O vínculo com o resultado é feito pelo agregado (AddLimit).</summary>
    public static CreditInquiryResultLimit Create(
        string groupName,
        string groupType,
        decimal availableLimit,
        decimal revisedLimit,
        decimal rate)
        => new()
        {
            GroupName = groupName,
            GroupType = groupType,
            AvailableLimit = availableLimit,
            RevisedLimit = revisedLimit,
            Rate = rate,
        };

    /// <summary>Vincula o limite ao resultado dono — chamado exclusivamente pelo agregado.</summary>
    internal void AttachTo(Guid creditInquiryResultId)
        => CreditInquiryResultId = creditInquiryResultId;
}
