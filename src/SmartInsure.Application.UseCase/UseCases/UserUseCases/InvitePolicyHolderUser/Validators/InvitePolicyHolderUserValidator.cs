using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Requests;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Validators;

/// <summary>RN-070: validação de forma da criação de Usuário no Tomador ativo.</summary>
public sealed class InvitePolicyHolderUserValidator : AbstractValidator<InvitePolicyHolderUserRequest>
{
    public InvitePolicyHolderUserValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.ProfileId).NotEmpty();

        // RN-082: CPF obrigatório e válido (11 dígitos).
        RuleFor(request => request.DocumentNumber)
            .NotEmpty().WithMessage("O CPF do usuário é obrigatório.")
            .Must(CpfValidator.IsValid).WithMessage("CPF deve conter 11 dígitos válidos.");
    }
}
