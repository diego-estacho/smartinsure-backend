using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Validators;

/// <summary>RN-051 — validação de forma da atualização do Grupo de Cotação em Rascunho.</summary>
public sealed class UpdateQuotationGroupValidator : AbstractValidator<UpdateQuotationGroupRequest>
{
    public UpdateQuotationGroupValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("O grupo de cotação é obrigatório.");

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

        RuleFor(request => request.InsurerIds)
            .NotEmpty().WithMessage("Selecione ao menos uma seguradora para o escopo específico.")
            .When(request => string.Equals(
                request.ScopeMode, nameof(EQuotationScopeMode.Specific), StringComparison.OrdinalIgnoreCase));
    }
}
