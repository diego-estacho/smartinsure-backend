using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface ILegalNatureRepository : IRepository<LegalNature>
{
    /// <summary>RN-014/RN-015: natureza localizada pelo código CONCLA (somente dígitos).</summary>
    Task<LegalNature?> GetByCodeAsync(string code, CancellationToken cancellationToken);
}
