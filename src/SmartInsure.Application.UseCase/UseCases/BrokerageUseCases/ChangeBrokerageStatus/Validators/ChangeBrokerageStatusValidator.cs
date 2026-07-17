using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Validators;

/// <summary>RN-021 — situação alvo da Corretora por nome estável.</summary>
public sealed class ChangeBrokerageStatusValidator : AbstractValidator<ChangeBrokerageStatusRequest>
{
    public ChangeBrokerageStatusValidator()
    {
        RuleFor(request => request.Status)
            .Must(status => Enum.TryParse<EPersonRoleStatus>(status, ignoreCase: true, out _))
            .WithMessage("A situação deve ser Active ou Inactive.");
    }
}
