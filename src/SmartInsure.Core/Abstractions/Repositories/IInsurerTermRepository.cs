using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

/// <summary>
/// Repositório do Termo da Seguradora (RN-506). A conclusão da unidade de trabalho é do UseCase via
/// <see cref="IUnitOfWork"/> (ADR-036).
/// </summary>
public interface IInsurerTermRepository : IRepository<InsurerTerm>
{
    /// <summary>
    /// RN-506: Termo vigente da Seguradora — o texto a exibir e a registrar no aceite. Nulo significa
    /// Seguradora sem Termo cadastrado, e nesse caso a emissão é bloqueada.
    /// </summary>
    Task<InsurerTerm?> GetActiveByInsurerAsync(Guid insurerId, CancellationToken cancellationToken);
}
