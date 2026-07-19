using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.CreateInsurerEnablement.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.CreateInsurerEnablement.Validators;

/// <summary>RN-022 — validação de forma da Habilitação de Seguradora.</summary>
public sealed class CreateInsurerEnablementValidator : AbstractValidator<CreateInsurerEnablementRequest>
{
    public CreateInsurerEnablementValidator()
    {
        RuleFor(request => request.BrokerageId)
            .NotEmpty().WithMessage("A corretora é obrigatória.");

        RuleFor(request => request.InsurerId)
            .NotEmpty().WithMessage("A seguradora é obrigatória.");

        RuleFor(request => request.CalculationEngine)
            .NotEmpty().WithMessage("O motor de cálculo é obrigatório.")
            .Must(engine => Enum.TryParse<ECalculationEngine>(engine, ignoreCase: true, out _))
            .WithMessage("O motor de cálculo informado não está disponível na plataforma.");
    }
}
