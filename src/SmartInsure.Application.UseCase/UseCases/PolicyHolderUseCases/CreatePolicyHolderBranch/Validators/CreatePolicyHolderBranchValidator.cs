using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch.Requests;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch.Validators;

public sealed class CreatePolicyHolderBranchValidator : AbstractValidator<CreatePolicyHolderBranchRequest>
{
    public CreatePolicyHolderBranchValidator()
    {
        RuleFor(request => request.PolicyHolderId)
            .NotEmpty().WithMessage("ID do tomador é obrigatório.");

        RuleFor(request => request.DocumentNumber)
            .NotEmpty().WithMessage("CNPJ é obrigatório.")
            .Must(CnpjValidator.IsValid).WithMessage("CNPJ deve conter 14 dígitos válidos.");
    }
}
