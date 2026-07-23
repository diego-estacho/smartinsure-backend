using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator.Requests;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator.Validators;

/// <summary>RN-036: validação de forma do convite de Corretor Administrador.</summary>
public sealed class InviteBrokerageAdministratorValidator
    : AbstractValidator<InviteBrokerageAdministratorRequest>
{
    public InviteBrokerageAdministratorValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
        RuleFor(request => request.Email).NotEmpty().EmailAddress();

        // RN-036: pelo menos uma Corretora precisa ser informada no ato do convite.
        RuleFor(request => request.BrokerageIds)
            .NotNull()
            .Must(brokerageIds => brokerageIds is { Count: > 0 })
            .WithMessage("Informe ao menos uma corretora para o convite.");
    }
}
