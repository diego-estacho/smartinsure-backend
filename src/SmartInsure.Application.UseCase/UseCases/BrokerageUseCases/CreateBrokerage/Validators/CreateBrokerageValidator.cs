using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Requests;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Validators;

/// <summary>RN-019 — criação de Corretora exige CNPJ válido.</summary>
public sealed class CreateBrokerageValidator : AbstractValidator<CreateBrokerageRequest>
{
    public CreateBrokerageValidator()
    {
        RuleFor(request => request.Cnpj)
            .NotEmpty().WithMessage("O CNPJ da corretora é obrigatório.")
            .Must(CnpjValidator.IsValid).WithMessage("O CNPJ da corretora é inválido.");

        // RN-034: e-mail de contato é opcional, mas se informado deve ter formato válido.
        RuleFor(request => request.ContactEmail)
            .EmailAddress().WithMessage("O e-mail de contato é inválido.")
            .When(request => !string.IsNullOrWhiteSpace(request.ContactEmail));
    }
}
