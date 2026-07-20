using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderAppointment.Requests;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderAppointment.Validators;

public sealed class CreatePolicyHolderAppointmentValidator : AbstractValidator<CreatePolicyHolderAppointmentRequest>
{
    public CreatePolicyHolderAppointmentValidator()
    {
        RuleFor(request => request.PolicyHolderId)
            .NotEmpty().WithMessage("ID do tomador é obrigatório.");

        RuleFor(request => request.BrokerageId)
            .NotEmpty().WithMessage("ID da corretora é obrigatório.");

        RuleFor(request => request.InsurerId)
            .NotEmpty().WithMessage("ID da seguradora é obrigatório.");
    }
}
