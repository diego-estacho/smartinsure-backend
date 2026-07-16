using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Validators;

public sealed class SearchPersonsValidator : AbstractValidator<SearchPersonsRequest>
{
    public SearchPersonsValidator()
    {
        RuleFor(request => request.Term)
            .NotEmpty().WithMessage("O termo de busca é obrigatório.");

        RuleFor(request => request.Role)
            .Must(role => Enum.TryParse<EPersonRole>(role, ignoreCase: false, out _))
            .WithMessage("O papel da busca deve ser Insured, Broker ou PolicyHolder.");
    }
}
