using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile.Requests;

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile.Validators;

/// <summary>RN-069/RN-070: validação de forma da criação de Perfil customizado.</summary>
public sealed class CreateScopedProfileValidator : AbstractValidator<CreateScopedProfileRequest>
{
    public CreateScopedProfileValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);

        // RN-062: Perfil sem nenhuma Permissão é válido — a lista pode vir vazia, mas não nula.
        RuleFor(request => request.PermissionCodes).NotNull();
    }
}
