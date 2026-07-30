using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Motivo de indisponibilidade/recusa de uma Cotação (RN-056/RN-058). Source distingue o que veio da
/// Seguradora (Provider) do que a plataforma gerou (Local — ex.: Seguradora habilitada não incluída
/// na solicitação no modo Specific).
/// </summary>
public sealed class QuotationReason : EntityBase
{
    private QuotationReason()
    {
    }

    public Guid QuotationId { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public EQuotationReasonSource Source { get; private set; }

    public static QuotationReason Create(Guid quotationId, string text, EQuotationReasonSource source)
        => new()
        {
            QuotationId = quotationId,
            Text = text,
            Source = source,
        };
}
