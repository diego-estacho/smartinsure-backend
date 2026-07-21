using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.UpdateModalityGroup.Requests;

namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.UpdateModalityGroup.Validators;

/// <summary>RN-029 — validação de forma da edição de Grupo de Modalidade.</summary>
public sealed class UpdateModalityGroupValidator : AbstractValidator<UpdateModalityGroupRequest>
{
    public UpdateModalityGroupValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("O nome do grupo é obrigatório.")
            .MaximumLength(200).WithMessage("O nome do grupo deve ter no máximo 200 caracteres.");

        RuleFor(request => request.Description)
            .MaximumLength(1000).WithMessage("A descrição deve ter no máximo 1000 caracteres.");

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("A ordem de exibição não pode ser negativa.");
    }
}
