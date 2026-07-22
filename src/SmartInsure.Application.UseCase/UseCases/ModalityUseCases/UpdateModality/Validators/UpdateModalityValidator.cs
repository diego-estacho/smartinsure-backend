using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Requests;

namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Validators;

/// <summary>RN-029 — validação de forma da edição de Modalidade.</summary>
public sealed class UpdateModalityValidator : AbstractValidator<UpdateModalityRequest>
{
    public UpdateModalityValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("O nome da modalidade é obrigatório.")
            .MaximumLength(200).WithMessage("O nome da modalidade deve ter no máximo 200 caracteres.");

        RuleFor(request => request.Description)
            .MaximumLength(1000).WithMessage("A descrição deve ter no máximo 1000 caracteres.");
    }
}
