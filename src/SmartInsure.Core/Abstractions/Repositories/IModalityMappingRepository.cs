using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IModalityMappingRepository : IRepository<ModalityMapping>
{
    /// <summary>RN-032/RN-034: já existe mapeamento Confirmado para a Modalidade Importada?</summary>
    Task<bool> HasConfirmedAsync(Guid importedModalityId, CancellationToken cancellationToken);

    /// <summary>RN-033: mapeamentos Confirmados com Modalidade Importada Ativa (a matriz do Mapa).</summary>
    Task<IReadOnlyList<ConfirmedMappingDto>> ListConfirmedActiveAsync(CancellationToken cancellationToken);
}
