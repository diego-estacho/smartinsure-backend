using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Requests;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Validators;

/// <summary>RN-032 — a consulta de CNPJ exige um CNPJ válido.</summary>
public sealed class PreviewBrokerageByCnpjValidator : AbstractValidator<PreviewBrokerageByCnpjRequest>
{
    public PreviewBrokerageByCnpjValidator()
    {
        RuleFor(request => request.Cnpj)
            .NotEmpty().WithMessage("O CNPJ da corretora é obrigatório.")
            .Must(CnpjValidator.IsValid).WithMessage("O CNPJ da corretora é inválido.");
    }
}
