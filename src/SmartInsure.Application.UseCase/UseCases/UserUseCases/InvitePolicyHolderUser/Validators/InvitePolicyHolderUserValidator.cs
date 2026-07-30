using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Requests;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Validators;

/// <summary>RN-070: validação de forma da criação de Usuário no Tomador ativo.</summary>
public sealed class InvitePolicyHolderUserValidator : AbstractValidator<InvitePolicyHolderUserRequest>
{
    public InvitePolicyHolderUserValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.ProfileId).NotEmpty();
    }
}
