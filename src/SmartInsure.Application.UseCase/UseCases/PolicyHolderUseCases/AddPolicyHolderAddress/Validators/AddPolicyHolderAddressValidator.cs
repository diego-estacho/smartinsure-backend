using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.AddPolicyHolderAddress.Requests;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.AddPolicyHolderAddress.Validators;

public sealed class AddPolicyHolderAddressValidator : AbstractValidator<AddPolicyHolderAddressRequest>
{
    public AddPolicyHolderAddressValidator()
    {
        RuleFor(request => request.PolicyHolderId)
            .NotEmpty().WithMessage("ID do tomador é obrigatório.");
    }
}
