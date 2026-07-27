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
}
