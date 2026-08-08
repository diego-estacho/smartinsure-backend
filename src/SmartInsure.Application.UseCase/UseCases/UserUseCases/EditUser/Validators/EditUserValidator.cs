using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.EditUser.Requests;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.EditUser.Validators;

/// <summary>RN-202: valida a forma da edição (o servidor decide a regra — Pendente, unicidade).</summary>
public sealed class EditUserValidator : AbstractValidator<EditUserRequest>
{
    public EditUserValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("O nome do usuário é obrigatório.")
            .MaximumLength(200).WithMessage("O nome do usuário deve ter no máximo 200 caracteres.");

        // E-mail é opcional na edição (nulo = não altera); quando informado, precisa ser válido.
        RuleFor(request => request.Email)
            .EmailAddress().WithMessage("O e-mail do usuário é inválido.")
            .MaximumLength(320).WithMessage("O e-mail do usuário deve ter no máximo 320 caracteres.")
            .When(request => !string.IsNullOrWhiteSpace(request.Email));
    }
}
