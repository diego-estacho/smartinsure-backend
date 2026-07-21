using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.CreateModalityGroup.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.CreateModalityGroup.Validators;

/// <summary>RN-029 — validação de forma do cadastro de Grupo de Modalidade.</summary>
public sealed class CreateModalityGroupValidator : AbstractValidator<CreateModalityGroupRequest>
{
    public CreateModalityGroupValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("O nome do grupo é obrigatório.")
            .MaximumLength(200).WithMessage("O nome do grupo deve ter no máximo 200 caracteres.");

        RuleFor(request => request.Description)
            .MaximumLength(1000).WithMessage("A descrição deve ter no máximo 1000 caracteres.");

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("A ordem de exibição não pode ser negativa.");

        RuleFor(request => request.InitialStatus)
            .Must(status => Enum.TryParse<EModalityGroupStatus>(status, ignoreCase: true, out _))
            .WithMessage("A situação inicial deve ser Active ou Inactive.");
    }
}
