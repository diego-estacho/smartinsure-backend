using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.UpdateBrokerageInsurerEnablement.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.UpdateBrokerageInsurerEnablement.Validators;

/// <summary>RN-022 — validação de forma da alteração da Habilitação.</summary>
public sealed class UpdateBrokerageInsurerEnablementValidator : AbstractValidator<UpdateBrokerageInsurerEnablementRequest>
{
    public UpdateBrokerageInsurerEnablementValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("A habilitação é obrigatória.");

        RuleFor(request => request.CalculationEngine)
            .NotEmpty().WithMessage("O motor de cálculo é obrigatório.")
            .Must(engine => Enum.TryParse<ECalculationEngine>(engine, ignoreCase: true, out _))
            .WithMessage("O motor de cálculo informado não está disponível na plataforma.");
    }
}
