using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.RemovePolicyHolderAddress.Requests;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.RemovePolicyHolderAddress.Validators;

public sealed class RemovePolicyHolderAddressValidator : AbstractValidator<RemovePolicyHolderAddressRequest>
{
    public RemovePolicyHolderAddressValidator()
    {
        RuleFor(request => request.PolicyHolderId)
            .NotEmpty().WithMessage("ID do tomador é obrigatório.");

        RuleFor(request => request.AddressId)
            .NotEmpty().WithMessage("ID do endereço é obrigatório.");
    }
}
