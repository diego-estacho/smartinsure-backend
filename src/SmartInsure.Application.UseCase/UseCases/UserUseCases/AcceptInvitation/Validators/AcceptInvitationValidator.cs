using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Requests;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Validators;

public sealed class AcceptInvitationValidator : AbstractValidator<AcceptInvitationRequest>
{
    public AcceptInvitationValidator()
    {
        RuleFor(r => r.Token)
            .NotEmpty().WithErrorCode("TokenRequired");

        RuleFor(r => r.Password)
            .NotEmpty().WithErrorCode("PasswordRequired")
            .MinimumLength(8).WithErrorCode("PasswordTooShort");
    }
}
