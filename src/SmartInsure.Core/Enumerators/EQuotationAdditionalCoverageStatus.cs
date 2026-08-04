namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Situação de uma Cobertura Adicional escolhida dentro de uma Cotação (RN-105/RN-106), exposta por
/// nome estável: enviada à Seguradora, ou não contemplada porque ela não oferece a cobertura na
/// Modalidade cotada (ou porque o nome divergiu entre ramos — OPEN-22).
/// </summary>
public enum EQuotationAdditionalCoverageStatus
{
    Sent,
    NotOffered,
}
