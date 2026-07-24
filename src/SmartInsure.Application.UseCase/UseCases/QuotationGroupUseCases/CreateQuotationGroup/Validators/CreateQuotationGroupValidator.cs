using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Validators;

/// <summary>RN-050 — validação de forma do Grupo de Cotação (a de negócio é do servidor, ADR-004).</summary>
public sealed class CreateQuotationGroupValidator : AbstractValidator<CreateQuotationGroupRequest>
{
    public CreateQuotationGroupValidator()
    {
        RuleFor(request => request.PolicyHolderId)
            .NotEmpty().WithMessage("O tomador é obrigatório.");

        RuleFor(request => request.InsuredId)
            .NotEmpty().WithMessage("O segurado é obrigatório.");

        RuleFor(request => request.ModalityId)
            .NotEmpty().WithMessage("A modalidade é obrigatória.");

        RuleFor(request => request.InsuredAmount)
            .GreaterThan(0).WithMessage("A importância segurada deve ser maior que zero.");

        RuleFor(request => request.CoverageEndDate)
            .GreaterThan(request => request.CoverageStartDate)
            .WithMessage("O fim da vigência deve ser posterior ao início.");

        RuleFor(request => request.ScopeMode)
            .NotEmpty().WithMessage("O escopo de seguradoras é obrigatório.")
            .Must(mode => Enum.TryParse<EQuotationScopeMode>(mode, ignoreCase: true, out _))
            .WithMessage("O escopo de seguradoras informado é inválido.");

        RuleForEach(request => request.InsurerIds)
            .NotEmpty().WithMessage("Seguradora inválida no escopo.");

        // Escopo Specific exige ao menos uma Seguradora escolhida.
        RuleFor(request => request.InsurerIds)
            .NotEmpty().WithMessage("Selecione ao menos uma seguradora para o escopo específico.")
            .When(request => string.Equals(
                request.ScopeMode, nameof(EQuotationScopeMode.Specific), StringComparison.OrdinalIgnoreCase));
    }
}
