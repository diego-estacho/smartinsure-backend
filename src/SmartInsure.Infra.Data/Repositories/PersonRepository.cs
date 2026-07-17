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
