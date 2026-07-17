using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.Services.PersonImports;

public sealed class PersonBureauImporter(
    ILegalNatureRepository legalNatureRepository,
    IBureauProvider bureauProvider) : IPersonBureauImporter
{
    public async Task<PersonBureauImport?> ImportLegalPersonAsync(
        string cnpj,
        EPersonRole role,
        CancellationToken cancellationToken)
    {
        var complement = await bureauProvider.GetPersonComplementAsync(
            cnpj, PersonTypeName(role), EBureau.ReceitaWS, cancellationToken);

        // RN-004/RN-014: consulta sem dado não bloqueia; nada é gravado.
        if (complement is null || string.IsNullOrWhiteSpace(complement.Name))
        {
            return null;
        }

        var legalNature = await ResolveLegalNatureAsync(complement, cancellationToken);

        var person = Person.Create(cnpj, complement.Name, complement.TradeName, legalNature.Id);
        person.AssignRole(role);
        person.AddMainAddress(
            complement.ZipCode,
            complement.Street,
            complement.Number,
            complement.AddressComplement,
            complement.District,
            complement.City,
            complement.State);

        return new PersonBureauImport(person, legalNature.IsPrivate);
    }

    private static string PersonTypeName(EPersonRole role)
        => role switch
        {
            EPersonRole.Insured => "Segurado",
            EPersonRole.Broker => "Corretor",
            _ => "Tomador",
        };

    private async Task<LegalNature> ResolveLegalNatureAsync(
        BureauPersonComplement complement,
        CancellationToken cancellationToken)
    {
        var code = new string([.. (complement.LegalNature ?? string.Empty).Where(char.IsDigit)]);

        var legalNature = string.IsNullOrEmpty(code)
            ? null
            : await legalNatureRepository.GetByCodeAsync(code, cancellationToken);

        return legalNature
            ?? throw new BusinessRuleException(
                "A natureza jurídica retornada pela fonte não está catalogada na plataforma.");
    }
}
