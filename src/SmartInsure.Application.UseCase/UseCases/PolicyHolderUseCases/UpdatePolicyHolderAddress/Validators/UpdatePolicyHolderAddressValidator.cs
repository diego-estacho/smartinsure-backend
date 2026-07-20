using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.UpdatePolicyHolderAddress.Requests;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.UpdatePolicyHolderAddress.Validators;

public sealed class UpdatePolicyHolderAddressValidator : AbstractValidator<UpdatePolicyHolderAddressRequest>
{
    public UpdatePolicyHolderAddressValidator()
    {
        RuleFor(request => request.PolicyHolderId)
            .NotEmpty().WithMessage("ID do tomador é obrigatório.");

        RuleFor(request => request.AddressId)
            .NotEmpty().WithMessage("ID do endereço é obrigatório.");
    }
}
