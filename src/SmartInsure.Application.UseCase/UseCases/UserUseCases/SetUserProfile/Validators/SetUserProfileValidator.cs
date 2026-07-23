using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Requests;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Validators;

/// <summary>
/// Validador de forma para concessão/revogação de perfil. O nome nulo revoga; quando informado,
/// não pode ser vazio — a existência do Perfil é resolvida no caso de uso (RN-012/RN-032).
/// </summary>
public sealed class SetUserProfileValidator : AbstractValidator<SetUserProfileRequest>
{
    public SetUserProfileValidator()
    {
        RuleFor(request => request.Profile)
            .Must(profile => profile is null || !string.IsNullOrWhiteSpace(profile))
            .WithMessage("O perfil deve ser SystemAdministrator ou nulo para revogação.");
    }
}
