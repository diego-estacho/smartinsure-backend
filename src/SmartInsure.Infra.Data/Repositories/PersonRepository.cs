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
            query = query.Where(person => person.Type == EPersonType.Legal
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

    private static IQueryable<PersonSearchItemDto> ProjectItems(IQueryable<Person> query)
        => query.Select(person => new PersonSearchItemDto(
            person.Id,
            person.DocumentNumber,
            person.Name,
            person.SocialName,
            person.Type.ToString(),
            person.LegalNature == null ? null : (bool?)person.LegalNature.IsPrivate,
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
