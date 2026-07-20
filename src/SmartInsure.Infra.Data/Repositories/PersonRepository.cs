using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class PersonRepository(SmartInsureDbContext context)
    : Repository<Person>(context), IPersonRepository
{
    public async Task<IReadOnlyList<PersonSearchItemDto>> SearchByNameOrDocumentAsync(
        string nameTerm,
        string? documentNumber,
        bool headquartersOnly,
        CancellationToken cancellationToken)
    {
        var query = Set.AsNoTracking()
            .Where(person => person.Name.Contains(nameTerm)
                || (person.SocialName != null && person.SocialName.Contains(nameTerm))
                || (documentNumber != null && person.DocumentNumber == documentNumber));

        if (headquartersOnly)
        {
            // RN-016: tomador é sempre a matriz (pessoa jurídica de ordem /0001).
            query = query.Where(person => person.Type == EPersonType.J
                && person.DocumentNumber.Substring(8, 4) == "0001");
        }

        return await ProjectItems(query)
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<PersonSearchItemDto?> GetByDocumentNumberAsync(
        string documentNumber, CancellationToken cancellationToken)
        => await ProjectItems(Set.AsNoTracking().Where(person => person.DocumentNumber == documentNumber))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Person?> GetTrackedByDocumentNumberAsync(
        string documentNumber, CancellationToken cancellationToken)
        => await Set
            .Include(person => person.Roles)
            .FirstOrDefaultAsync(
                person => person.DocumentNumber == documentNumber, cancellationToken);

    public async Task<(IReadOnlyList<BrokerageListItemDto> Items, long TotalCount)> ListBrokeragesAsync(
        int page,
        int pageSize,
        EPersonRoleStatus? status,
        CancellationToken cancellationToken)
    {
        var query = Set.AsNoTracking()
            .Where(person => person.Type == EPersonType.J
                && person.Roles.Any(role => role.Role == EPersonRole.Broker
                    && (status == null || role.Status == status)));

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderBy(person => person.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(person => new BrokerageListItemDto(
                person.Id,
                person.DocumentNumber,
                person.Name,
                person.SocialName,
                person.LegalNature == null ? null : (bool?)person.LegalNature.IsPrivate,
                person.Roles
                    .Where(role => role.Role == EPersonRole.Broker)
                    .Select(role => role.Status.ToString())
                    .First()))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<BrokerageDetailsDto?> GetBrokerageByIdAsync(
        Guid personId,
        CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .Where(person => person.Id == personId
                && person.Type == EPersonType.J
                && person.Roles.Any(role => role.Role == EPersonRole.Broker))
            .Select(person => new BrokerageDetailsDto(
                person.Id,
                person.DocumentNumber,
                person.Name,
                person.SocialName,
                person.LegalNature == null ? null : person.LegalNature.Code,
                person.LegalNature == null ? null : person.LegalNature.Name,
                person.LegalNature == null ? null : (bool?)person.LegalNature.IsPrivate,
                person.Roles
                    .Where(role => role.Role == EPersonRole.Broker)
                    .Select(role => role.Status.ToString())
                    .First(),
                person.Addresses
                    .Where(address => address.IsMain)
                    .Select(address => new PersonMainAddressDto(
                        address.ZipCode,
                        address.Street,
                        address.Number,
                        address.Complement,
                        address.Neighborhood,
                        address.City,
                        address.State))
                    .FirstOrDefault()))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Person?> GetTrackedBrokerageByIdAsync(
        Guid personId,
        CancellationToken cancellationToken)
        => await Set
            .Include(person => person.Roles)
            .FirstOrDefaultAsync(
                person => person.Id == personId
                    && person.Type == EPersonType.J
                    && person.Roles.Any(role => role.Role == EPersonRole.Broker),
                cancellationToken);

    public async Task<(IReadOnlyList<PolicyHolderListItemDto> Items, long TotalCount)> ListPolicyHoldersAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken)
    {
        var query = Set.AsNoTracking()
            .Where(person => person.Type == EPersonType.J
                && person.Roles.Any(role => role.Role == EPersonRole.PolicyHolder)
                && person.DocumentNumber.Substring(8, 4) == "0001");

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(person => person.Name.Contains(searchTerm)
                || (person.SocialName != null && person.SocialName.Contains(searchTerm))
                || person.DocumentNumber.Contains(searchTerm));
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderBy(person => person.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(person => new PolicyHolderListItemDto(
                person.Id,
                person.DocumentNumber,
                person.Name,
                person.SocialName,
                person.LegalNature == null ? null : (bool?)person.LegalNature.IsPrivate))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<PolicyHolderDetailsDto?> GetPolicyHolderByIdAsync(
        Guid personId,
        CancellationToken cancellationToken)
    {
        var person = await Set.AsNoTracking()
            .Where(p => p.Id == personId
                && p.Type == EPersonType.J
                && p.Roles.Any(role => role.Role == EPersonRole.PolicyHolder)
                && p.DocumentNumber.Substring(8, 4) == "0001")
            .Select(p => new
            {
                p.Id,
                p.DocumentNumber,
                p.Name,
                p.SocialName,
                LegalNatureCode = p.LegalNature == null ? null : p.LegalNature.Code,
                LegalNatureDescription = p.LegalNature == null ? null : p.LegalNature.Name,
                IsPrivateSector = p.LegalNature == null ? null : (bool?)p.LegalNature.IsPrivate,
                Addresses = p.Addresses.Select(a => new PersonAddressDetailsDto(
                    a.Id,
                    a.ZipCode,
                    a.Street,
                    a.Number,
                    a.Complement,
                    a.Neighborhood,
                    a.City,
                    a.State,
                    a.IsMain)).ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (person is null)
        {
            return null;
        }

        var appointments = await Context.Set<PolicyHolderAppointment>().AsNoTracking()
            .Where(appointment => appointment.PolicyHolderId == personId)
            .OrderByDescending(appointment => appointment.StartedAt)
            .Select(appointment => new PolicyHolderAppointmentDetailDto(
                appointment.Id,
                appointment.InsurerId,
                Context.Set<Insurer>()
                    .Where(insurer => insurer.Id == appointment.InsurerId)
                    .Select(insurer => insurer.Cnpj)
                    .First(),
                Context.Set<Insurer>()
                    .Where(insurer => insurer.Id == appointment.InsurerId)
                    .Select(insurer => insurer.CorporateName)
                    .First(),
                appointment.BrokerageId,
                Context.Set<Person>()
                    .Where(p => p.Id == appointment.BrokerageId)
                    .Select(p => p.DocumentNumber)
                    .First(),
                Context.Set<Person>()
                    .Where(p => p.Id == appointment.BrokerageId)
                    .Select(p => p.Name)
                    .First(),
                appointment.Status.ToString(),
                appointment.StartedAt,
                appointment.EndedAt))
            .ToListAsync(cancellationToken);

        return new PolicyHolderDetailsDto(
            person.Id,
            person.DocumentNumber,
            person.Name,
            person.SocialName,
            person.LegalNatureCode,
            person.LegalNatureDescription,
            person.IsPrivateSector,
            person.Addresses,
            appointments);
    }

    public async Task<Person?> GetTrackedPolicyHolderByIdAsync(
        Guid personId,
        CancellationToken cancellationToken)
        => await Set
            .Include(person => person.Roles)
            .Include(person => person.Addresses)
            .FirstOrDefaultAsync(
                person => person.Id == personId
                    && person.Type == EPersonType.J
                    && person.Roles.Any(role => role.Role == EPersonRole.PolicyHolder)
                    && person.DocumentNumber.Substring(8, 4) == "0001",
                cancellationToken);

    private static IQueryable<PersonSearchItemDto> ProjectItems(IQueryable<Person> query)
        => query.Select(person => new PersonSearchItemDto(
            person.Id,
            person.DocumentNumber,
            person.Name,
            person.SocialName,
            person.Type.ToString(),
            person.LegalNature == null ? null : (bool?)person.LegalNature.IsPrivate,
            person.Roles.Select(role => role.Role.ToString()).ToList(),
            person.Addresses
                .Where(address => address.IsMain)
                .Select(address => new PersonMainAddressDto(
                    address.ZipCode,
                    address.Street,
                    address.Number,
                    address.Complement,
                    address.Neighborhood,
                    address.City,
                    address.State))
                .FirstOrDefault()));
}
