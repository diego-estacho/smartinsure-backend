using FluentAssertions;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Validators;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Validators;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Validators;

namespace SmartInsure.Tests.Application.UseCases.ModalityUseCases;

/// <summary>RN-029 — validação de forma do cadastro de Modalidade.</summary>
[Trait("RuleId", "RN-029")]
public class CreateModalityValidatorTests
{
    private readonly CreateModalityValidator _validator = new();

    private static CreateModalityRequest Request(
        string name = "Garantia de Execução de Contrato",
        Guid? modalityGroupId = null,
        string? description = "Performance",
        string initialStatus = "Active")
        => new(name, modalityGroupId ?? Guid.CreateVersion7(), description, initialStatus);

    [Fact]
    public void Validate_DeveAprovar_QuandoRequestValido()
        => _validator.Validate(Request()).IsValid.Should().BeTrue();

    [Fact]
    public void Validate_DeveRecusar_QuandoNomeAusente()
        => _validator.Validate(Request(name: "")).IsValid.Should().BeFalse();

    [Fact]
    public void Validate_DeveRecusar_QuandoGrupoAusente()
        => _validator.Validate(Request(modalityGroupId: Guid.Empty)).IsValid.Should().BeFalse();

    [Fact]
    public void Validate_DeveRecusar_QuandoSituacaoInicialDesconhecida()
        => _validator.Validate(Request(initialStatus: "Rascunho")).IsValid.Should().BeFalse();
}

/// <summary>RN-029 — validação de forma da edição de Modalidade.</summary>
[Trait("RuleId", "RN-029")]
public class UpdateModalityValidatorTests
{
    private readonly UpdateModalityValidator _validator = new();

    [Fact]
    public void Validate_DeveAprovar_QuandoRequestValido()
        => _validator.Validate(new UpdateModalityRequest(Guid.CreateVersion7(), "Garantia", Guid.CreateVersion7(), null))
            .IsValid.Should().BeTrue();

    [Fact]
    public void Validate_DeveRecusar_QuandoGrupoAusente()
        => _validator.Validate(new UpdateModalityRequest(Guid.CreateVersion7(), "Garantia", Guid.Empty, null))
            .IsValid.Should().BeFalse();
}

/// <summary>RN-036 — validação de forma da alteração de situação de Modalidade.</summary>
[Trait("RuleId", "RN-036")]
public class ChangeModalityStatusValidatorTests
{
    private readonly ChangeModalityStatusValidator _validator = new();

    [Fact]
    public void Validate_DeveAprovar_QuandoSituacaoConhecida()
        => _validator.Validate(new ChangeModalityStatusRequest(Guid.CreateVersion7(), "Active"))
            .IsValid.Should().BeTrue();

    [Fact]
    public void Validate_DeveRecusar_QuandoSituacaoDesconhecida()
        => _validator.Validate(new ChangeModalityStatusRequest(Guid.CreateVersion7(), "Cancelada"))
            .IsValid.Should().BeFalse();
}
