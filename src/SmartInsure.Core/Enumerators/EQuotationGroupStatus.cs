namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Situação do Grupo de Cotação (glossário; RN-050/RN-051/RN-508). Persistido como string (ADR-031).
/// Nesta fase são três: Rascunho (montado no wizard), Cotado (Cotações obtidas das Seguradoras) e
/// Emissão solicitada (emissão da Cotação escolhida pedida à Seguradora e registrada). A situação
/// Emitida — Apólice confirmada pela Seguradora, com número e arquivo — só entra com a confirmação da
/// emissão, demanda própria (OPEN-07): a plataforma não afirma emissão que não confirmou.
/// </summary>
public enum EQuotationGroupStatus
{
    Draft,
    Quoted,
    EmissionRequested,
}
