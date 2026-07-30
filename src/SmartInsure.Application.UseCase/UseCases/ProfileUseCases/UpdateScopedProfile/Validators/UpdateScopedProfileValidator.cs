using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile.Requests;

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile.Validators;

/// <summary>RN-074: validação de forma da edição de Perfil customizado.</summary>
public sealed class UpdateScopedProfileValidator : AbstractValidator<UpdateScopedProfileRequest>
{
    public UpdateScopedProfileValidator()
    {
        RuleFor(request => request.ProfileId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
        RuleFor(request => request.PermissionCodes).NotNull();
    }
}
