using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.MapImportedModality.Requests;

namespace SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.MapImportedModality.Validators;

/// <summary>RN-034 — validação de forma do mapeamento manual.</summary>
public sealed class MapImportedModalityValidator : AbstractValidator<MapImportedModalityRequest>
{
    public MapImportedModalityValidator()
    {
        RuleFor(request => request.ImportedModalityId)
            .NotEmpty().WithMessage("A modalidade importada é obrigatória.");

        RuleFor(request => request.ModalityId)
            .NotEmpty().WithMessage("A modalidade de destino é obrigatória.");
    }
}
