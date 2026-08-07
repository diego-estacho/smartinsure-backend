namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Requests;

/// <summary>Dados de entrada para criar o Grupo de Cotação em Rascunho ao concluir a etapa de risco (RN-050).</summary>
/// <param name="PolicyHolderId">Tomador (Pessoa com papel PolicyHolder).</param>
/// <param name="BranchId">RN-102: estabelecimento cotado — Filial do Tomador; ausente significa a matriz.</param>
/// <param name="InsuredId">Segurado (Pessoa com papel Insured).</param>
/// <param name="ModalityId">Modalidade escolhida no catálogo do Smart.</param>
/// <param name="InsuredAmount">Importância segurada (valor segurado).</param>
/// <param name="CoverageStartDate">Início da vigência.</param>
/// <param name="CoverageEndDate">Fim da vigência.</param>
/// <param name="ScopeMode">Escopo de Seguradoras pelo nome estável: All ou Specific.</param>
/// <param name="InsurerIds">Seguradoras escolhidas quando o escopo é Specific.</param>
/// <param name="AdditionalCoverageIds">RN-104: Coberturas Adicionais canônicas escolhidas na etapa de risco.</param>
/// <param name="InsuredAddressId">
/// RN-503: endereço do Segurado escolhido pelo corretor, entre os do cadastro da Pessoa. Ausente
/// significa o endereço principal. O Grupo guarda uma RÉPLICA dos valores, não a referência.
/// </param>
public sealed record CreateQuotationGroupRequest(
    Guid PolicyHolderId,
    Guid? BranchId,
    Guid InsuredId,
    Guid ModalityId,
    decimal InsuredAmount,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    string ScopeMode,
    IReadOnlyList<Guid> InsurerIds,
    IReadOnlyList<Guid> AdditionalCoverageIds,
    Guid? InsuredAddressId = null);
