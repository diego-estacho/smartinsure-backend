using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ChangeModalityGroupStatus.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ChangeModalityGroupStatus.Validators;

/// <summary>RN-036 — validação de forma da alteração de situação de Grupo de Modalidade.</summary>
public sealed class ChangeModalityGroupStatusValidator : AbstractValidator<ChangeModalityGroupStatusRequest>
{
    public ChangeModalityGroupStatusValidator()
    {
        RuleFor(request => request.Status)
            .NotEmpty().WithMessage("A situação deve ser Active ou Inactive.")
            .Must(status => Enum.TryParse<EModalityGroupStatus>(status, ignoreCase: true, out _))
            .WithMessage("A situação deve ser Active ou Inactive.");
    }
}
