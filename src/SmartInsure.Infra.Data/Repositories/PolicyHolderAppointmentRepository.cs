using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class PolicyHolderAppointmentRepository(SmartInsureDbContext context)
    : Repository<PolicyHolderAppointment>(context), IPolicyHolderAppointmentRepository
{
    public async Task<PolicyHolderAppointment?> GetTrackedByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
        => await Set.FirstOrDefaultAsync(
            appointment => appointment.Id == id, cancellationToken);

    public async Task<PolicyHolderAppointment?> GetTrackedActiveByPairAsync(
        Guid policyHolderId,
        Guid insurerId,
        CancellationToken cancellationToken)
        => await Set.FirstOrDefaultAsync(
            appointment => appointment.PolicyHolderId == policyHolderId
                && appointment.InsurerId == insurerId
                && appointment.Status == EPolicyHolderAppointmentStatus.Active,
            cancellationToken);

    public async Task<IReadOnlyList<PolicyHolderAppointmentDetailDto>> ListByPolicyHolderAsync(
        Guid policyHolderId,
        CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .Where(appointment => appointment.PolicyHolderId == policyHolderId)
            .OrderByDescending(appointment => appointment.StartedAt)
            .Select(appointment => new PolicyHolderAppointmentDetailDto(
                appointment.Id,
                appointment.InsurerId,
                context.Set<Insurer>()
                    .Where(insurer => insurer.Id == appointment.InsurerId)
                    .Select(insurer => insurer.Cnpj)
                    .First(),
                context.Set<Insurer>()
                    .Where(insurer => insurer.Id == appointment.InsurerId)
                    .Select(insurer => insurer.CorporateName)
                    .First(),
                appointment.BrokerageId,
                context.Set<Person>()
                    .Where(person => person.Id == appointment.BrokerageId)
                    .Select(person => person.DocumentNumber)
                    .First(),
                context.Set<Person>()
                    .Where(person => person.Id == appointment.BrokerageId)
                    .Select(person => person.Name)
                    .First(),
                appointment.Status.ToString(),
                appointment.StartedAt,
                appointment.EndedAt))
            .ToListAsync(cancellationToken);
}
