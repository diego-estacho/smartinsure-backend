using FluentValidation;

namespace SmartInsure.Application.UseCase.UseCases.Users.CreateUser;

public sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
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
