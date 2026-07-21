namespace SmartInsure.Core.Entities;

/// <summary>
/// Consulta de Crédito (RN-029..031): operação em que o usuário consulta os Limites de Crédito
/// de um CNPJ de tomador junto às Seguradoras habilitadas da Corretora.
/// Cada execução gera um registro histórico com data/hora e resultados (inclusive indisponibilidades).
/// Imutável: nunca editado nem excluído (RN-031); criada apenas via factory.
/// </summary>
public sealed class CreditInquiry : EntityBase
{
    private readonly List<CreditInquiryResult> _results = [];

    private CreditInquiry()
    {
    }

    public Guid BrokerageId { get; private set; }

    public string PolicyHolderCnpj { get; private set; } = string.Empty;

    public DateTime QueriedAt { get; private set; }

    public IReadOnlyCollection<CreditInquiryResult> Results => _results.AsReadOnly();

    /// <summary>RN-029/RN-031: cria nova consulta de crédito com data/hora na criação.</summary>
    public static CreditInquiry Create(Guid brokerageId, string policyHolderCnpj)
        => new()
        {
            BrokerageId = brokerageId,
            PolicyHolderCnpj = policyHolderCnpj,
            QueriedAt = DateTime.UtcNow,
        };

    /// <summary>RN-030/RN-031: adiciona resultado (disponível ou indisponível) à consulta.</summary>
    public void AddResult(CreditInquiryResult result)
    {
        if (result.CreditInquiryId != Id)
        {
            throw new InvalidOperationException("O resultado não pertence a esta consulta de crédito.");
        }

        _results.Add(result);
    }
}
