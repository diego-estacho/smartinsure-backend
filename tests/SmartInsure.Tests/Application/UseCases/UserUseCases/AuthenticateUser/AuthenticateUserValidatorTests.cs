using FluentAssertions;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Validators;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.AuthenticateUser;

/// <summary>RN-005 — validação de forma da autenticação.</summary>
[Trait("RuleId", "RN-005")]
public class AuthenticateUserValidatorTests
{
    private readonly AuthenticateUserValidator _validator = new();

    [Fact]
    public void Validate_DeveAprovar_QuandoEmailESenhaInformados()
    {
        var result = _validator.Validate(
            new AuthenticateUserRequest("maria@corretora.com.br", "senha"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-email")]
    public void Validate_DeveReprovar_QuandoEmailAusenteOuInvalido(string email)
    {
        var result = _validator.Validate(new AuthenticateUserRequest(email, "senha"));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_DeveReprovar_QuandoSenhaAusente()
    {
        var result = _validator.Validate(
            new AuthenticateUserRequest("maria@corretora.com.br", ""));

        result.IsValid.Should().BeFalse();
    }
}
