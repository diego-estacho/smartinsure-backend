namespace SmartInsure.Core.Entities;

/// <summary>
/// Cobertura Adicional canônica escolhida num Grupo de Cotação (RN-104). Substitui os booleanos
/// provisórios IncludesPenaltyCoverage/IncludesLaborCoverage. É conjunto: uma cobertura aparece uma
/// única vez por Grupo. O que a Seguradora recebe é resolvido na solicitação da Cotação (RN-105).
/// </summary>
public sealed class QuotationGroupAdditionalCoverage : EntityBase
{
    private QuotationGroupAdditionalCoverage()
    {
    }

    public Guid QuotationGroupId { get; private set; }

    public Guid AdditionalCoverageId { get; private set; }

    public static QuotationGroupAdditionalCoverage Create(Guid quotationGroupId, Guid additionalCoverageId)
        => new()
        {
            QuotationGroupId = quotationGroupId,
            AdditionalCoverageId = additionalCoverageId,
        };
}
