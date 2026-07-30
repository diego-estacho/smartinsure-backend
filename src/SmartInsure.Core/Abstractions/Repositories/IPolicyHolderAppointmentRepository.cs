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

    /// <summary>
    /// RN-068: existe Nomeação Vigente do Tomador em que esta Corretora é a nomeada, em
    /// qualquer Seguradora? É a pré-condição para o Corretor Administrador criar um
    /// Tomador Administrador daquele Tomador.
    /// </summary>
    Task<bool> ExistsActiveForPolicyHolderAndBrokerageAsync(
        Guid policyHolderId,
        Guid brokerageId,
        CancellationToken cancellationToken);

    /// <summary>RN-025: lista todas as Nomeações (Vigentes e Encerradas) do Tomador.</summary>
    Task<IReadOnlyList<PolicyHolderAppointmentDetailDto>> ListByPolicyHolderAsync(
        Guid policyHolderId,
        CancellationToken cancellationToken);
}
