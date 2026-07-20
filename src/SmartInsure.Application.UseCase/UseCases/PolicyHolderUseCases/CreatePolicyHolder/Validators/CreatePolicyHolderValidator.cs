using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolder.Requests;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolder.Validators;

public sealed class CreatePolicyHolderValidator : AbstractValidator<CreatePolicyHolderRequest>
{
    public CreatePolicyHolderValidator()
    {
        RuleFor(request => request.Cnpj)
            .NotEmpty().WithMessage("CNPJ é obrigatório.")
            .Must(CnpjValidator.IsValid).WithMessage("CNPJ deve conter 14 dígitos válidos.");
    }
}
