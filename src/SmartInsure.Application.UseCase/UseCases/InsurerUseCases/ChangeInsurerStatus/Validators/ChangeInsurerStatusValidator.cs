using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Validators;

/// <summary>RN-007 — validação de forma da alteração de situação de Seguradora.</summary>
public sealed class ChangeInsurerStatusValidator : AbstractValidator<ChangeInsurerStatusRequest>
{
    public ChangeInsurerStatusValidator()
    {
        RuleFor(request => request.Status)
            .NotEmpty().WithMessage("A situação deve ser Active ou Inactive.")
            .Must(status => Enum.TryParse<EInsurerStatus>(status, ignoreCase: true, out _))
            .WithMessage("A situação deve ser Active ou Inactive.");
    }
}
