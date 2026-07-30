using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Requests;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Validators;

/// <summary>RN-069: validação de forma da criação de Usuário na Corretora ativa.</summary>
public sealed class InviteBrokerageUserValidator : AbstractValidator<InviteBrokerageUserRequest>
{
    public InviteBrokerageUserValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.ProfileId).NotEmpty();
    }
}
