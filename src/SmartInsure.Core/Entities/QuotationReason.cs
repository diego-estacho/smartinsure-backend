namespace SmartInsure.Core.Entities;

/// <summary>
/// Motivo informado pela Seguradora para uma Cotação Indisponível/Recusada (RN-058, ADR-064).
/// Lista que acompanha o resultado como dado — motivo novo do parceiro não cria status novo.
/// Filho do agregado <see cref="Quotation"/>.
/// </summary>
public sealed class QuotationReason : EntityBase
{
    private QuotationReason()
    {
    }

    public Guid QuotationId { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public static QuotationReason Create(string text)
        => new()
        {
            Text = text,
        };

    internal void AttachTo(Guid quotationId) => QuotationId = quotationId;
}
