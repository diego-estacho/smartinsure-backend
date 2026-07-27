using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.CreateAdditionalCoverage.Requests;

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.CreateAdditionalCoverage.Validators;

/// <summary>RN-040 — validação de forma do cadastro de Cobertura Adicional.</summary>
public sealed class CreateAdditionalCoverageValidator : AbstractValidator<CreateAdditionalCoverageRequest>
{
    public CreateAdditionalCoverageValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("O nome da Cobertura Adicional é obrigatório.")
            .MaximumLength(300).WithMessage("O nome da Cobertura Adicional deve ter no máximo 300 caracteres.");
    }
}
