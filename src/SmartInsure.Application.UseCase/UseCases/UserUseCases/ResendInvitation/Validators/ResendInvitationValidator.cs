using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Requests;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Validators;

public sealed class ResendInvitationValidator : AbstractValidator<ResendInvitationRequest>
{
    public ResendInvitationValidator()
    {
        RuleFor(r => r.UserId)
            .NotEmpty().WithErrorCode("UserIdRequired");
    }
}
