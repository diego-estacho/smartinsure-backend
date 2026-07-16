using FluentAssertions;
using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Requests;
using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Validators;

namespace SmartInsure.Tests.Application.UseCases.PersonUseCases.SearchPersons;

/// <summary>RN-013 — a busca exige termo e papel de contexto válidos.</summary>
[Trait("RuleId", "RN-013")]
public class SearchPersonsValidatorTests
{
    private readonly SearchPersonsValidator _validator = new();

    [Theory]
    [InlineData("Alfa", "Insured", true)]
    [InlineData("11444777000161", "PolicyHolder", true)]
    [InlineData("", "Insured", false)]
    [InlineData("Alfa", "", false)]
    [InlineData("Alfa", "Corretora", false)]
    public void Validate_DeveAceitarSomenteTermoEPapelValidos(string term, string role, bool expected)
    {
        var result = _validator.Validate(new SearchPersonsRequest(term, role));

        result.IsValid.Should().Be(expected);
    }
}
