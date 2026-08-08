using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Requests;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Validators;

/// <summary>RN-069: validação de forma da criação de Usuário na Corretora ativa.</summary>
public sealed class InviteBrokerageUserValidator : AbstractValidator<InviteBrokerageUserRequest>
{
    public InviteBrokerageUserValidator()
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
