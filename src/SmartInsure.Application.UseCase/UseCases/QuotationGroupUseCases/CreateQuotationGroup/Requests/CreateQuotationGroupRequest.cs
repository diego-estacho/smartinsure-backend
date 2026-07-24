namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Requests;

/// <summary>Dados de entrada para criar o Grupo de Cotação em Rascunho ao concluir a etapa de risco (RN-050).</summary>
/// <param name="PolicyHolderId">Tomador (Pessoa com papel PolicyHolder).</param>
/// <param name="InsuredId">Segurado (Pessoa com papel Insured).</param>
/// <param name="ModalityId">Modalidade escolhida no catálogo do Smart.</param>
/// <param name="InsuredAmount">Importância segurada (valor segurado).</param>
/// <param name="CoverageStartDate">Início da vigência.</param>
/// <param name="CoverageEndDate">Fim da vigência.</param>
/// <param name="ScopeMode">Escopo de Seguradoras pelo nome estável: All ou Specific.</param>
/// <param name="InsurerIds">Seguradoras escolhidas quando o escopo é Specific.</param>
/// <param name="IncludesPenaltyCoverage">Cobertura de Multa marcada (provisório).</param>
/// <param name="IncludesLaborCoverage">Cobertura Trabalhista/Previdenciária marcada (provisório).</param>
public sealed record CreateQuotationGroupRequest(
    Guid PolicyHolderId,
    Guid InsuredId,
    Guid ModalityId,
    decimal InsuredAmount,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    string ScopeMode,
    IReadOnlyList<Guid> InsurerIds,
    bool IncludesPenaltyCoverage,
    bool IncludesLaborCoverage);
