using FluentAssertions;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Validators;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.SetUserProfile;

/// <summary>RN-012 — validação de forma da concessão/revogação de perfil de usuário.</summary>
[Trait("RuleId", "RN-012")]
public class SetUserProfileValidatorTests
{
    private readonly SetUserProfileValidator _validator = new();

    private static SetUserProfileRequest Request(
        Guid? userId = null,
        string? profile = "SystemAdministrator")
        => new(userId ?? Guid.NewGuid(), profile);

    [Fact]
    public void Validate_DeveAprovar_QuandoProfileNulo()
        => _validator.Validate(Request(profile: null)).IsValid.Should().BeTrue();

    [Fact]
    public void Validate_DeveAprovar_QuandoProfileInformado()
        => _validator.Validate(Request(profile: "SystemAdministrator")).IsValid.Should().BeTrue();

    // A validade do nome do Perfil é resolvida no caso de uso (RN-012/RN-032), não no validador de forma;
    // aqui só recusamos nome em branco.
    [Fact]
    public void Validate_DeveRecusar_QuandoProfileEmBranco()
        => _validator.Validate(Request(profile: "   ")).IsValid.Should().BeFalse();
}
