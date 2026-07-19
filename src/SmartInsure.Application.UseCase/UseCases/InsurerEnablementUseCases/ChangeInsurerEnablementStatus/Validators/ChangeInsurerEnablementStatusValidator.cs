using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ChangeInsurerEnablementStatus.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ChangeInsurerEnablementStatus.Validators;

/// <summary>RN-022 — validação de forma da alternância de situação.</summary>
public sealed class ChangeInsurerEnablementStatusValidator
    : AbstractValidator<ChangeInsurerEnablementStatusRequest>
{
    public ChangeInsurerEnablementStatusValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("A habilitação é obrigatória.");

        RuleFor(request => request.Status)
            .Must(status => Enum.TryParse<EInsurerEnablementStatus>(status, ignoreCase: true, out _))
            .WithMessage("A situação deve ser Active ou Inactive.");
    }
}
