using FluentAssertions;
using SmartInsure.Application.UseCase.UseCases.Users.CreateUser;

namespace SmartInsure.Tests.Application.UseCases.Users;

/// <summary>RN-001 — casos limite de dados obrigatórios e inválidos.</summary>
[Trait("RuleId", "RN-001")]
public class CreateUserValidatorTests
{
    private static readonly CreateUserValidator Validator = new();

    [Theory]
    [InlineData("", "maria@corretora.com.br")]
    [InlineData("   ", "maria@corretora.com.br")]
    [InlineData("Maria Silva", "")]
    [InlineData("Maria Silva", "email-invalido")]
    public void Validate_DeveRecusar_QuandoNomeOuEmailAusenteOuInvalido(string name, string email)
        => Validator.Validate(new CreateUserRequest(name, email))
            .IsValid.Should().BeFalse();

    [Fact]
    public void Validate_DeveAceitar_QuandoNomeEEmailValidos()
        => Validator.Validate(new CreateUserRequest("Maria Silva", "maria@corretora.com.br"))
            .IsValid.Should().BeTrue();
}
