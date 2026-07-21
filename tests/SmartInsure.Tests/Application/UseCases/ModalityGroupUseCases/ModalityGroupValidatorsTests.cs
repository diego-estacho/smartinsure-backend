using FluentAssertions;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ChangeModalityGroupStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ChangeModalityGroupStatus.Validators;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.CreateModalityGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.CreateModalityGroup.Validators;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.UpdateModalityGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.UpdateModalityGroup.Validators;

namespace SmartInsure.Tests.Application.UseCases.ModalityGroupUseCases;

/// <summary>RN-029 — validação de forma do cadastro de Grupo de Modalidade.</summary>
[Trait("RuleId", "RN-029")]
public class CreateModalityGroupValidatorTests
{
    private readonly CreateModalityGroupValidator _validator = new();

    private static CreateModalityGroupRequest Request(
        string name = "Garantias de Licitação",
        string? description = "Bid bonds",
        int displayOrder = 1,
        string initialStatus = "Active")
        => new(name, description, displayOrder, initialStatus);

    [Fact]
    public void Validate_DeveAprovar_QuandoRequestValido()
        => _validator.Validate(Request()).IsValid.Should().BeTrue();

    [Fact]
    public void Validate_DeveRecusar_QuandoNomeAusente()
        => _validator.Validate(Request(name: "")).IsValid.Should().BeFalse();

    [Fact]
    public void Validate_DeveRecusar_QuandoOrdemNegativa()
        => _validator.Validate(Request(displayOrder: -1)).IsValid.Should().BeFalse();

    [Fact]
    public void Validate_DeveRecusar_QuandoSituacaoInicialDesconhecida()
        => _validator.Validate(Request(initialStatus: "Suspenso")).IsValid.Should().BeFalse();

    [Fact]
    public void Validate_DeveAprovar_QuandoDescricaoAusente()
        => _validator.Validate(Request(description: null)).IsValid.Should().BeTrue();
}

/// <summary>RN-029 — validação de forma da edição de Grupo de Modalidade.</summary>
[Trait("RuleId", "RN-029")]
public class UpdateModalityGroupValidatorTests
{
    private readonly UpdateModalityGroupValidator _validator = new();

    [Fact]
    public void Validate_DeveAprovar_QuandoRequestValido()
        => _validator.Validate(new UpdateModalityGroupRequest(Guid.CreateVersion7(), "Garantias", "Desc", 0))
            .IsValid.Should().BeTrue();

    [Fact]
    public void Validate_DeveRecusar_QuandoNomeAusente()
        => _validator.Validate(new UpdateModalityGroupRequest(Guid.CreateVersion7(), "", null, 0))
            .IsValid.Should().BeFalse();

    [Fact]
    public void Validate_DeveRecusar_QuandoOrdemNegativa()
        => _validator.Validate(new UpdateModalityGroupRequest(Guid.CreateVersion7(), "Garantias", null, -5))
            .IsValid.Should().BeFalse();
}

/// <summary>RN-036 — validação de forma da alteração de situação de Grupo de Modalidade.</summary>
[Trait("RuleId", "RN-036")]
public class ChangeModalityGroupStatusValidatorTests
{
    private readonly ChangeModalityGroupStatusValidator _validator = new();

    [Fact]
    public void Validate_DeveAprovar_QuandoSituacaoConhecida()
        => _validator.Validate(new ChangeModalityGroupStatusRequest(Guid.CreateVersion7(), "Inactive"))
            .IsValid.Should().BeTrue();

    [Fact]
    public void Validate_DeveRecusar_QuandoSituacaoDesconhecida()
        => _validator.Validate(new ChangeModalityGroupStatusRequest(Guid.CreateVersion7(), "Arquivado"))
            .IsValid.Should().BeFalse();
}
