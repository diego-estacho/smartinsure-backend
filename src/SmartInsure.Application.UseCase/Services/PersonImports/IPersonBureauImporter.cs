using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.Services.PersonImports;

public interface IPersonBureauImporter
{
    Task<PersonBureauImport?> ImportLegalPersonAsync(
        string cnpj,
        EPersonRole role,
        CancellationToken cancellationToken);
}

public sealed record PersonBureauImport(Person Person, bool IsPrivateSector);
