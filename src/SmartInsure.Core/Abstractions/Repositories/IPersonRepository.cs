using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IPersonRepository : IRepository<Person>
{
    /// <summary>
    /// RN-013: busca por "contém" no nome e no nome social, ou por documento exato
    /// (CPF/CNPJ, somente dígitos). RN-016: <paramref name="headquartersOnly"/>
    /// restringe a matrizes (pessoas jurídicas de ordem /0001).
    /// </summary>
    Task<IReadOnlyList<PersonSearchItemDto>> SearchByNameOrDocumentAsync(
        string nameTerm,
        string? documentNumber,
        bool headquartersOnly,
        CancellationToken cancellationToken);

    /// <summary>RN-014: uma Pessoa por documento — consulta antes de importar.</summary>
    Task<PersonSearchItemDto?> GetByDocumentNumberAsync(
        string documentNumber, CancellationToken cancellationToken);

    /// <summary>RN-017: entidade rastreada para atribuição de papel via change tracker.</summary>
    Task<Person?> GetTrackedByDocumentNumberAsync(
        string documentNumber, CancellationToken cancellationToken);

    /// <summary>Pessoa por id com os Papéis carregados, para conferir o Papel exigido (RN-013/RN-017).</summary>
    Task<Person?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Resumo da Pessoa por id (nome, documento, nome social e endereço principal), para reidratar o Grupo de Cotação (RN-051).</summary>
    Task<PersonSearchItemDto?> GetSummaryByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// RN-018: lista Pessoas jurídicas com Papel da Pessoa de corretor, com busca, filtros
    /// combinados (situação, seguradora, motor, setor, período) e contagem por situação — tudo no servidor.
    /// </summary>
    Task<BrokerageListResult> ListBrokeragesAsync(
        BrokerageListQuery query,
        CancellationToken cancellationToken);

    /// <summary>RN-020: detalhe da Corretora a partir da Pessoa jurídica com papel Corretor.</summary>
    Task<BrokerageDetailsDto?> GetBrokerageByIdAsync(
        Guid personId,
        CancellationToken cancellationToken);

    /// <summary>RN-055: linha do tempo da Corretora derivada da auditoria (criação, habilitações, última edição).</summary>
    Task<IReadOnlyList<BrokerageHistoryEventDto>> GetBrokerageHistoryAsync(
        Guid personId,
        CancellationToken cancellationToken);

    /// <summary>RN-101: dados de um CNPJ já cadastrado (somente leitura), para a consulta do cadastro.</summary>
    Task<BrokeragePreviewDto?> FindBrokeragePreviewByDocumentAsync(
        string documentNumber,
        CancellationToken cancellationToken);

    /// <summary>RN-021: Pessoa rastreada com o papel Corretor para alterar situação.</summary>
    Task<Person?> GetTrackedBrokerageByIdAsync(
        Guid personId,
        CancellationToken cancellationToken);

    /// <summary>
    /// RN-025/RN-200: lista Pessoas jurídicas com papel Tomador, filtradas por search opcional.
    /// Quando <paramref name="brokerageId"/> é informado, cada item indica se o Tomador já tem
    /// Nomeação Vigente com a Corretora ativa (RN-200).
    /// </summary>
    Task<(IReadOnlyList<PolicyHolderListItemDto> Items, long TotalCount)> ListPolicyHoldersAsync(
        int page,
        int pageSize,
        string? search,
        Guid? brokerageId,
        CancellationToken cancellationToken);

    /// <summary>RN-025: detalhes do Tomador a partir da Pessoa jurídica com papel PolicyHolder, incluindo endereços e nomeações.</summary>
    Task<PolicyHolderDetailsDto?> GetPolicyHolderByIdAsync(
        Guid personId,
        CancellationToken cancellationToken);

    /// <summary>RN-025/026: Pessoa rastreada com o papel PolicyHolder para alterar endereços.</summary>
    Task<Person?> GetTrackedPolicyHolderByIdAsync(
        Guid personId,
        CancellationToken cancellationToken);

    /// <summary>RN-101: Pessoa rastreada por id, para vincular a Filial à matriz.</summary>
    Task<Person?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>RN-101: Filiais vinculadas a uma matriz, ordenadas por documento.</summary>
    Task<IReadOnlyList<PersonBranchDto>> ListBranchesAsync(
        Guid headquartersPersonId, CancellationToken cancellationToken);
}
