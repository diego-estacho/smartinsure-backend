using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.Services.PersonImports;

public interface IPersonBureauImporter
{
    /// <summary>
    /// RN-014: importa a Pessoa jurídica do Birô. <paramref name="assignRole"/> falso importa sem
    /// atribuir Papel da Pessoa — caso do preview de Corretora (RN-101) e da Filial no cadastro em
    /// cadeia (RN-101/ADR-101); <paramref name="role"/> segue definindo o rótulo enviado ao Birô.
    /// </summary>
    Task<PersonBureauImport?> ImportLegalPersonAsync(
        string cnpj,
        EPersonRole role,
        bool assignRole,
        CancellationToken cancellationToken);
}

public sealed record PersonBureauImport(
    Person Person,
    bool IsPrivateSector,
    string? LegalNatureCode,
    string? LegalNatureName);
