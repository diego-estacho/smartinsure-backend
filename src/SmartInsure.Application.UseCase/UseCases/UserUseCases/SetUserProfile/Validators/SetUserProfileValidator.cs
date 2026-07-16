using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Requests;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Validators;

/// <summary>Validador de regras de negócio para concessão/revogação de perfil.</summary>
public sealed class SetUserProfileValidator : AbstractValidator<SetUserProfileRequest>
{
    /// <summary>Inicializa o validador com as regras de negócio para concessão/revogação de perfil.</summary>
    public SetUserProfileValidator()
    {
        RuleFor(request => request.Profile)
            .Custom((profile, context) =>
            {
                if (profile is null)
                {
                    return;
                }

                if (!Enum.TryParse<EUserProfile>(profile, ignoreCase: true, out _))
                {
                    context.AddFailure("O perfil deve ser SystemAdministrator ou nulo para revogação.");
                }
            });
    }
}
