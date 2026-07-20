using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.EndPolicyHolderAppointment.Requests;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.EndPolicyHolderAppointment.Validators;

public sealed class EndPolicyHolderAppointmentValidator : AbstractValidator<EndPolicyHolderAppointmentRequest>
{
    public EndPolicyHolderAppointmentValidator()
    {
        RuleFor(request => request.AppointmentId)
            .NotEmpty().WithMessage("ID da nomeação é obrigatório.");
    }
}
