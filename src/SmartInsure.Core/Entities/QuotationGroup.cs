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

    /// <summary>RN-102: estabelecimento cotado — Filial escolhida; nulo significa a matriz (ADR-101).</summary>
    public Guid? BranchPersonId { get; private set; }

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
    /// RN-503: réplica do endereço do Segurado escolhido, feita na criação da oferta. É ela que abastece
    /// a emissão — não o cadastro da Pessoa, que pode mudar depois.
    /// </summary>
    public QuotationAddress? InsuredAddress { get; private set; }

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
        Guid? branchPersonId,
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
            BranchPersonId = branchPersonId,
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

    /// <summary>
    /// RN-508: o Grupo passa a Cotado quando as Cotações são obtidas das Seguradoras. Idempotente — o
    /// fan-out registra cada Cotação conforme chega (RN-057) e a primeira já promove a situação.
    /// </summary>
    public void MarkQuoted()
    {
        if (Status == EQuotationGroupStatus.EmissionRequested)
        {
            throw new InvalidOperationException(
                "Grupo de Cotação com emissão já solicitada não volta a Cotado (RN-508).");
        }

        Status = EQuotationGroupStatus.Quoted;
    }

    /// <summary>
    /// RN-508: registra que a emissão da Cotação escolhida foi solicitada à Seguradora. Só a partir de
    /// Cotado — não existe emitir sem leque —, e uma única vez (RN-507).
    /// </summary>
    public void MarkEmissionRequested()
    {
        if (Status != EQuotationGroupStatus.Quoted)
        {
            throw new InvalidOperationException(
                "Só um Grupo de Cotação Cotado pode ter emissão solicitada (RN-508).");
        }

        Status = EQuotationGroupStatus.EmissionRequested;
    }

    /// <summary>RN-508: com emissão solicitada, a oferta não aceita mais alteração.</summary>
    private void EnsureEmissionNotRequested(string action)
    {
        if (Status == EQuotationGroupStatus.EmissionRequested)
        {
            throw new InvalidOperationException(
                $"Grupo de Cotação com emissão solicitada não aceita {action} (RN-508).");
        }
    }

    /// <summary>RN-051: enquanto Rascunho, atualiza os dados no lugar (mesmo id); o estado não muda aqui.</summary>
    public void UpdateDraft(
        Guid policyHolderId,
        Guid? branchPersonId,
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
        EnsureEmissionNotRequested("alteração dos dados da oferta");

        PolicyHolderId = policyHolderId;
        BranchPersonId = branchPersonId;
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
    public void SelectQuotation(Guid quotationId)
    {
        EnsureEmissionNotRequested("troca da Cotação escolhida");

        SelectedQuotationId = quotationId;
    }

    /// <summary>RN-060: o recálculo descarta a escolha — o risco a que ela se referia deixou de valer.</summary>
    public void ClearSelection() => SelectedQuotationId = null;

    /// <summary>
    /// RN-057: registra a Corretora da solicitação corrente — o reconciliador (ADR-050) a usa para
    /// reenfileirar as Cotações que ficaram em Requested após restart.
    /// </summary>
    public void AssignBrokerage(Guid brokerageId) => BrokerageId = brokerageId;

    /// <summary>
    /// RN-503: replica para a oferta o endereço do Segurado escolhido pelo corretor. Chamada de novo,
    /// atualiza a réplica no lugar (é o mesmo endereço da oferta) — é o caminho de correção depois que o
    /// cadastro do Segurado foi ajustado. Não descarta Cotações nem cria Grupo novo: endereço não é
    /// dado-base (RN-060), pois não é enviado na cotação e não afeta prêmio, veredito ou limite.
    /// </summary>
    public void ReplicateInsuredAddress(
        string? zipCode,
        string? street,
        string? number,
        string? complement,
        string? neighborhood,
        string? city,
        string? state)
    {
        if (InsuredAddress is null)
        {
            InsuredAddress = QuotationAddress.Replicate(
                Id, zipCode, street, number, complement, neighborhood, city, state);
        }
        else
        {
            InsuredAddress.Update(zipCode, street, number, complement, neighborhood, city, state);
        }

        if (!InsuredAddress.IsUsableForIssuance())
        {
            throw new InvalidOperationException(
                "O endereço do Segurado precisa de CEP, logradouro, cidade e UF para emitir (RN-503).");
        }
    }

    /// <summary>RN-503: a oferta tem endereço do Segurado replicado e utilizável para emitir.</summary>
    public bool HasInsuredAddressForIssuance()
        => InsuredAddress is not null && InsuredAddress.IsUsableForIssuance();
}
