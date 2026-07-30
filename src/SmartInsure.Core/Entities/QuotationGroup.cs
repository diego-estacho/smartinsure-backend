using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Grupo de Cotação (RN-050/RN-051): o pedido/estudo que o corretor monta no wizard de nova oferta
/// (tomador, segurado, escopo de Seguradoras, modalidade, valor segurado, vigência e coberturas).
/// Nasce em Rascunho ao concluir a etapa de risco; enquanto Rascunho é atualizado no lugar (mesmo id).
/// A UI o chama de "oferta" (rótulo provisório). Cotar as Seguradoras e emitir seguem fora de escopo (OPEN-07).
/// </summary>
public sealed class QuotationGroup : EntityBase
{
    private readonly List<QuotationGroupInsurer> _selectedInsurers = [];

    private QuotationGroup()
    {
    }

    public Guid PolicyHolderId { get; private set; }

    public Guid InsuredId { get; private set; }

    public Guid ModalityId { get; private set; }

    public decimal InsuredAmount { get; private set; }

    public DateOnly CoverageStartDate { get; private set; }

    public DateOnly CoverageEndDate { get; private set; }

    public EQuotationScopeMode ScopeMode { get; private set; }

    /// <summary>Cobertura Adicional de Multa marcada — provisório (2 booleanos até o read de coberturas por modalidade; RN-051).</summary>
    public bool IncludesPenaltyCoverage { get; private set; }

    /// <summary>Cobertura Adicional Trabalhista/Previdenciária marcada — provisório (RN-051).</summary>
    public bool IncludesLaborCoverage { get; private set; }

    public EQuotationGroupStatus Status { get; private set; }

    /// <summary>Cotação escolhida do Grupo para seguir (RN-059); nula enquanto nenhuma foi escolhida.</summary>
    public Guid? SelectedQuotationId { get; private set; }

    /// <summary>
    /// Corretora dona da última solicitação de Cotações (RN-023/OPEN-03) — origem do fan-out. Persistida
    /// para o reconciliador reconstruir o work item e reenfileirar as Cotações paradas em Requested após
    /// restart/deploy (ADR-050); nula enquanto o Grupo nunca foi cotado.
    /// </summary>
    public Guid? BrokerageId { get; private set; }

    /// <summary>Seguradoras do escopo, quando o modo é Specific (vazio quando All).</summary>
    public IReadOnlyCollection<QuotationGroupInsurer> SelectedInsurers => _selectedInsurers.AsReadOnly();

    /// <summary>RN-050: o Grupo de Cotação nasce em Rascunho ao concluir a etapa de risco.</summary>
    public static QuotationGroup Create(
        Guid policyHolderId,
        Guid insuredId,
        Guid modalityId,
        decimal insuredAmount,
        DateOnly coverageStartDate,
        DateOnly coverageEndDate,
        EQuotationScopeMode scopeMode,
        IEnumerable<Guid> insurerIds,
        bool includesPenaltyCoverage,
        bool includesLaborCoverage)
    {
        var group = new QuotationGroup
        {
            PolicyHolderId = policyHolderId,
            InsuredId = insuredId,
            ModalityId = modalityId,
            InsuredAmount = insuredAmount,
            CoverageStartDate = coverageStartDate,
            CoverageEndDate = coverageEndDate,
            ScopeMode = scopeMode,
            IncludesPenaltyCoverage = includesPenaltyCoverage,
            IncludesLaborCoverage = includesLaborCoverage,
            Status = EQuotationGroupStatus.Draft,
        };

        group.ReplaceSelectedInsurers(scopeMode, insurerIds);

        return group;
    }

    /// <summary>RN-051: enquanto Rascunho, atualiza os dados no lugar (mesmo id); o estado não muda aqui.</summary>
    public void UpdateDraft(
        Guid policyHolderId,
        Guid insuredId,
        Guid modalityId,
        decimal insuredAmount,
        DateOnly coverageStartDate,
        DateOnly coverageEndDate,
        EQuotationScopeMode scopeMode,
        IEnumerable<Guid> insurerIds,
        bool includesPenaltyCoverage,
        bool includesLaborCoverage)
    {
        PolicyHolderId = policyHolderId;
        InsuredId = insuredId;
        ModalityId = modalityId;
        InsuredAmount = insuredAmount;
        CoverageStartDate = coverageStartDate;
        CoverageEndDate = coverageEndDate;
        ScopeMode = scopeMode;
        IncludesPenaltyCoverage = includesPenaltyCoverage;
        IncludesLaborCoverage = includesLaborCoverage;

        ReplaceSelectedInsurers(scopeMode, insurerIds);
    }

    private void ReplaceSelectedInsurers(EQuotationScopeMode scopeMode, IEnumerable<Guid> insurerIds)
    {
        _selectedInsurers.Clear();

        // Escopo All cota todas as habilitadas (OPEN-07): não há Seguradoras específicas a guardar.
        if (scopeMode != EQuotationScopeMode.Specific)
        {
            return;
        }

        foreach (var insurerId in insurerIds.Distinct())
        {
            _selectedInsurers.Add(QuotationGroupInsurer.Create(Id, insurerId));
        }
    }

    /// <summary>
    /// RN-059: marca a Cotação escolhida do Grupo. No máximo uma por Grupo; escolher outra substitui a
    /// anterior. A validação de seguibilidade/posse é do use case (que tem a Cotação em mãos).
    /// </summary>
    public void SelectQuotation(Guid quotationId) => SelectedQuotationId = quotationId;

    /// <summary>RN-060: o recálculo descarta a escolha — o risco a que ela se referia deixou de valer.</summary>
    public void ClearSelection() => SelectedQuotationId = null;

    /// <summary>
    /// RN-057: registra a Corretora da solicitação corrente — o reconciliador (ADR-050) a usa para
    /// reenfileirar as Cotações que ficaram em Requested após restart.
    /// </summary>
    public void AssignBrokerage(Guid brokerageId) => BrokerageId = brokerageId;
}
