using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator.Requests;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator.Validators;

/// <summary>RN-068: validação de forma do convite de Tomador Administrador.</summary>
public sealed class InvitePolicyHolderAdministratorValidator
    : AbstractValidator<InvitePolicyHolderAdministratorRequest>
{
    public InvitePolicyHolderAdministratorValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.PolicyHolderId).NotEmpty();
    }
}
