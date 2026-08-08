using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.RequestPasswordReset.Requests;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.RequestPasswordReset.Validators;

public sealed class RequestPasswordResetValidator : AbstractValidator<RequestPasswordResetRequest>
{
    public RequestPasswordResetValidator()
    {
        RuleFor(r => r.UserId)
            .NotEmpty().WithErrorCode("UserIdRequired");
    }
}
