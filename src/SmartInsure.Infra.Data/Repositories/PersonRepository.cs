using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class PersonRepository(SmartInsureDbContext context)
    : Repository<Person>(context), IPersonRepository
{
    public async Task<IReadOnlyList<PersonSearchItemDto>> SearchByNameOrCnpjAsync(
        string nameTerm,
        string? cnpj,
        bool headquartersOnly,
        CancellationToken cancellationToken)
    {
        var query = Set.AsNoTracking()
            .Where(entity => entity.CorporateName.Contains(nameTerm)
                || (entity.TradeName != null && entity.TradeName.Contains(nameTerm))
                || (cnpj != null && entity.Cnpj == cnpj));

        if (headquartersOnly)
        {
            // RN-016: tomador é sempre a matriz (ordem /0001 do CNPJ).
            query = query.Where(entity => entity.Cnpj.Substring(8, 4) == "0001");
        }

        return await ProjectItems(query)
            .OrderBy(item => item.CorporateName)
            .ToListAsync(cancellationToken);
    }

    public async Task<PersonSearchItemDto?> GetByCnpjAsync(
        string cnpj, CancellationToken cancellationToken)
        => await ProjectItems(Set.AsNoTracking().Where(entity => entity.Cnpj == cnpj))
            .FirstOrDefaultAsync(cancellationToken);

    private static IQueryable<PersonSearchItemDto> ProjectItems(IQueryable<Person> query)
        => query.Select(entity => new PersonSearchItemDto(
            entity.Id,
            entity.Cnpj,
            entity.CorporateName,
            entity.TradeName,
            entity.LegalNature!.IsPrivate,
            entity.Addresses
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
