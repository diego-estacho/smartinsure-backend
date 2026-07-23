using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.ReassignImportedModality.Requests;

namespace SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.ReassignImportedModality.Validators;

/// <summary>RN-037 — validação de forma da reatribuição manual.</summary>
public sealed class ReassignImportedModalityValidator : AbstractValidator<ReassignImportedModalityRequest>
{
    public ReassignImportedModalityValidator()
    {
        RuleFor(request => request.ImportedModalityId)
            .NotEmpty().WithMessage("A modalidade importada é obrigatória.");

        RuleFor(request => request.ModalityId)
            .NotEmpty().WithMessage("A modalidade de destino é obrigatória.");
    }
}
