using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Requests;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Validators;

/// <summary>Validador de regras de negócio para criação de usuário.</summary>
public sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    /// <summary>Inicializa o validador com as regras de negócio para criação de usuário.</summary>
    public CreateUserValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("O nome do usuário é obrigatório.")
            .MaximumLength(200).WithMessage("O nome do usuário deve ter no máximo 200 caracteres.");

        RuleFor(request => request.Email)
            .NotEmpty().WithMessage("O e-mail do usuário é obrigatório.")
            .EmailAddress().WithMessage("O e-mail do usuário é inválido.")
            .MaximumLength(320).WithMessage("O e-mail do usuário deve ter no máximo 320 caracteres.");
    }
}
