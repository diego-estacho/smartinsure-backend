using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.LegalEntityUseCases.SearchLegalEntities.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.LegalEntityUseCases.SearchLegalEntities.Validators;

public sealed class SearchLegalEntitiesValidator : AbstractValidator<SearchLegalEntitiesRequest>
{
    public SearchLegalEntitiesValidator()
    {
        RuleFor(request => request.Term)
            .NotEmpty().WithMessage("O termo de busca é obrigatório.");

        RuleFor(request => request.Role)
            .Must(role => Enum.TryParse<ELegalEntityRole>(role, ignoreCase: false, out _))
            .WithMessage("O papel da busca deve ser Insured ou PolicyHolder.");
    }
}
