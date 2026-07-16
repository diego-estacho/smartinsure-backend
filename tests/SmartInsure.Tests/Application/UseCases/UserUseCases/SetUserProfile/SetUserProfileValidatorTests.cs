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
    public void Validate_DeveAprovar_QuandoProfileValido()
        => _validator.Validate(Request(profile: "SystemAdministrator")).IsValid.Should().BeTrue();

    [Fact]
    public void Validate_DeveRecusar_QuandoProfileInvalido()
        => _validator.Validate(Request(profile: "SuperUser")).IsValid.Should().BeFalse();
}
