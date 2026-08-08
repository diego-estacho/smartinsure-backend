using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator.Requests;
using SmartInsure.Infra.CrossCutting.Validators;

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

        // RN-082: CPF obrigatório e válido (11 dígitos).
        RuleFor(request => request.DocumentNumber)
            .NotEmpty().WithMessage("O CPF do usuário é obrigatório.")
            .Must(CpfValidator.IsValid).WithMessage("CPF deve conter 11 dígitos válidos.");
    }
}
