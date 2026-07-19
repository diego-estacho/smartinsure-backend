using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.UpdateInsurerEnablement.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.UpdateInsurerEnablement.Validators;

/// <summary>RN-022 — validação de forma da alteração da Habilitação.</summary>
public sealed class UpdateInsurerEnablementValidator : AbstractValidator<UpdateInsurerEnablementRequest>
{
    public UpdateInsurerEnablementValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("A habilitação é obrigatória.");

        RuleFor(request => request.CalculationEngine)
            .NotEmpty().WithMessage("O motor de cálculo é obrigatório.")
            .Must(engine => Enum.TryParse<ECalculationEngine>(engine, ignoreCase: true, out _))
            .WithMessage("O motor de cálculo informado não está disponível na plataforma.");
    }
}
