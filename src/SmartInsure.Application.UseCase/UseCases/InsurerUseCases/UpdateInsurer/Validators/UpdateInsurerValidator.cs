using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Requests;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Validators;

/// <summary>RN-006 — validação de forma da alteração cadastral de Seguradora.</summary>
public sealed class UpdateInsurerValidator : AbstractValidator<UpdateInsurerRequest>
{
    public UpdateInsurerValidator()
    {
        RuleFor(request => request.Cnpj)
            .NotEmpty().WithMessage("O CNPJ da seguradora é obrigatório.")
            .Must(CnpjValidator.IsValid).WithMessage("O CNPJ da seguradora é inválido.");

        RuleFor(request => request.CorporateName)
            .NotEmpty().WithMessage("A razão social da seguradora é obrigatória.")
            .MaximumLength(200).WithMessage("A razão social deve ter no máximo 200 caracteres.");

        RuleFor(request => request.TradeName)
            .MaximumLength(200).WithMessage("O nome fantasia deve ter no máximo 200 caracteres.");

        RuleFor(request => request.LogoUrl)
            .Must(BeAValidUrl).WithMessage("O endereço do logotipo é inválido.")
            .MaximumLength(500).WithMessage("O endereço do logotipo deve ter no máximo 500 caracteres.");
    }

    private static bool BeAValidUrl(string? url)
        => url is null
            || (Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps));
}
