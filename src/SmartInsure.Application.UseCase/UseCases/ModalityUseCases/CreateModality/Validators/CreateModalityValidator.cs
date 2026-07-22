using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Validators;

/// <summary>RN-029 — validação de forma do cadastro de Modalidade.</summary>
public sealed class CreateModalityValidator : AbstractValidator<CreateModalityRequest>
{
    public CreateModalityValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("O nome da modalidade é obrigatório.")
            .MaximumLength(200).WithMessage("O nome da modalidade deve ter no máximo 200 caracteres.");

        RuleFor(request => request.Description)
            .MaximumLength(1000).WithMessage("A descrição deve ter no máximo 1000 caracteres.");

        RuleFor(request => request.InitialStatus)
            .Must(status => Enum.TryParse<EModalityStatus>(status, ignoreCase: true, out _))
            .WithMessage("A situação inicial deve ser Active ou Inactive.");
    }
}
