using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserScopeProfile.Requests;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserScopeProfile.Validators;

/// <summary>RN-075: valida a forma; a pertinência do Perfil ao Escopo é decidida no use case.</summary>
public sealed class ChangeUserScopeProfileValidator : AbstractValidator<ChangeUserScopeProfileRequest>
{
    public ChangeUserScopeProfileValidator()
    {
        RuleFor(request => request.ScopeId).NotEmpty();
        RuleFor(request => request.ProfileId).NotEmpty();
    }
}
