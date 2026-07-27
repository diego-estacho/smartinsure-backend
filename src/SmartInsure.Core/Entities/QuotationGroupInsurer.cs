namespace SmartInsure.Core.Entities;

/// <summary>
/// Seguradora escolhida no escopo de um Grupo de Cotação, quando o modo é Specific (RN-050).
/// Vínculo simples Grupo×Seguradora; não existe para o escopo All (que cota todas as habilitadas).
/// </summary>
public sealed class QuotationGroupInsurer : EntityBase
{
    private QuotationGroupInsurer()
    {
    }

    public Guid QuotationGroupId { get; private set; }

    public Guid InsurerId { get; private set; }

    public static QuotationGroupInsurer Create(Guid quotationGroupId, Guid insurerId)
        => new()
        {
            QuotationGroupId = quotationGroupId,
            InsurerId = insurerId,
        };
}
