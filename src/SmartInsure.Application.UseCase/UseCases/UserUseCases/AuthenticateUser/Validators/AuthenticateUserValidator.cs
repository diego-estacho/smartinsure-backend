using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Requests;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Validators;

/// <summary>Validador de forma da autenticação (RN-005).</summary>
public sealed class AuthenticateUserValidator : AbstractValidator<AuthenticateUserRequest>
{
    /// <summary>Inicializa o validador com as regras de forma da autenticação.</summary>
    public AuthenticateUserValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail é inválido.")
            .MaximumLength(320).WithMessage("O e-mail deve ter no máximo 320 caracteres.");

        RuleFor(request => request.Password)
            .NotEmpty().WithMessage("A senha é obrigatória.");
    }
}
