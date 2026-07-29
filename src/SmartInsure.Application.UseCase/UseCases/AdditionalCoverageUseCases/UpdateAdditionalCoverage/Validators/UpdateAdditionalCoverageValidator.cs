using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.UpdateAdditionalCoverage.Requests;

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.UpdateAdditionalCoverage.Validators;

/// <summary>RN-040 — validação de forma da edição de Cobertura Adicional.</summary>
public sealed class UpdateAdditionalCoverageValidator : AbstractValidator<UpdateAdditionalCoverageRequest>
{
    public UpdateAdditionalCoverageValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("O nome da Cobertura Adicional é obrigatório.")
            .MaximumLength(300).WithMessage("O nome da Cobertura Adicional deve ter no máximo 300 caracteres.");
    }
}
