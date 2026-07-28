using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.Services.PersonImports;

public interface IPersonBureauImporter
{
    /// <summary>
    /// RN-014: importa a Pessoa jurídica do Birô. RN-052/ADR-063: <paramref name="role"/> nulo
    /// importa sem atribuir Papel da Pessoa — caso da Filial no cadastro em cadeia.
    /// </summary>
    Task<PersonBureauImport?> ImportLegalPersonAsync(
        string cnpj,
        EPersonRole? role,
        CancellationToken cancellationToken);
}

public sealed record PersonBureauImport(Person Person, bool IsPrivateSector);
