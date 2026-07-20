using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IPolicyHolderAppointmentRepository : IRepository<PolicyHolderAppointment>
{
    /// <summary>RN-027: busca entidade rastreada por id para alteração de status.</summary>
    Task<PolicyHolderAppointment?> GetTrackedByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    /// <summary>
    /// RN-027/RN-028: busca a Nomeação Vigente (Active) para o par Tomador×Seguradora.
    /// Retorna null se não houver vigente (novo par).
    /// </summary>
    Task<PolicyHolderAppointment?> GetTrackedActiveByPairAsync(
        Guid policyHolderId,
        Guid insurerId,
        CancellationToken cancellationToken);

    /// <summary>RN-025: lista todas as Nomeações (Vigentes e Encerradas) do Tomador.</summary>
    Task<IReadOnlyList<PolicyHolderAppointmentDetailDto>> ListByPolicyHolderAsync(
        Guid policyHolderId,
        CancellationToken cancellationToken);
}
