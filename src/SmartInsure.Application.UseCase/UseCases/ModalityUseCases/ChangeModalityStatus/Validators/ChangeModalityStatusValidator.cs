using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Validators;

/// <summary>RN-039 — validação de forma da alteração de situação de Modalidade.</summary>
public sealed class ChangeModalityStatusValidator : AbstractValidator<ChangeModalityStatusRequest>
{
    public ChangeModalityStatusValidator()
    {
        RuleFor(request => request.Status)
            .NotEmpty().WithMessage("A situação deve ser Active ou Inactive.")
            .Must(status => Enum.TryParse<EModalityStatus>(status, ignoreCase: true, out _))
            .WithMessage("A situação deve ser Active ou Inactive.");
    }
}
