using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ChangeBrokerageInsurerEnablementStatus.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ChangeBrokerageInsurerEnablementStatus.Validators;

/// <summary>RN-022 — validação de forma da alternância de situação.</summary>
public sealed class ChangeBrokerageInsurerEnablementStatusValidator
    : AbstractValidator<ChangeBrokerageInsurerEnablementStatusRequest>
{
    public ChangeBrokerageInsurerEnablementStatusValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("A habilitação é obrigatória.");

        RuleFor(request => request.Status)
            .Must(status => Enum.TryParse<EBrokerageInsurerEnablementStatus>(status, ignoreCase: true, out _))
            .WithMessage("A situação deve ser Active ou Inactive.");
    }
}
