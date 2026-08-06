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

        // RN-104: a coleção é obrigatória (lista vazia = nenhuma cobertura escolhida). Sem o NotNull,
        // corpo sem a chave chega como null e o RuleForEach passa em silêncio, virando 500 adiante.
        RuleFor(request => request.AdditionalCoverageIds)
            .NotNull().WithMessage("A lista de coberturas adicionais é obrigatória (use lista vazia para nenhuma).");

        // Id vazio na lista é forma inválida (a existência no catálogo é checada pelo servidor).
        RuleForEach(request => request.AdditionalCoverageIds)
            .NotEmpty().WithMessage("Cobertura adicional inválida.");

        RuleFor(request => request.InsurerIds)
            .NotEmpty().WithMessage("Selecione ao menos uma seguradora para o escopo específico.")
            .When(request => string.Equals(
                request.ScopeMode, nameof(EQuotationScopeMode.Specific), StringComparison.OrdinalIgnoreCase));
    }
}
