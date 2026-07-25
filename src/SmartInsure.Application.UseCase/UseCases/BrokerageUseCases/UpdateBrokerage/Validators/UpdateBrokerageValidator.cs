using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.UpdateBrokerage.Requests;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.UpdateBrokerage.Validators;

/// <summary>RN-034 — e-mail de contato opcional, mas com formato válido quando informado.</summary>
public sealed class UpdateBrokerageValidator : AbstractValidator<UpdateBrokerageRequest>
{
    public UpdateBrokerageValidator()
    {
        RuleFor(request => request.ContactEmail)
            .EmailAddress().WithMessage("O e-mail de contato é inválido.")
            .When(request => !string.IsNullOrWhiteSpace(request.ContactEmail));
    }
}
