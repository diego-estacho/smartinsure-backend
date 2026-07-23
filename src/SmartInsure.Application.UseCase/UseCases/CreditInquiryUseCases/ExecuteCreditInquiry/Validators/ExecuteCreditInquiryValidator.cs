using FluentValidation;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Requests;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Validators;

/// <summary>RN-029: validação de entrada da consulta de crédito — recusa CNPJ inválido antes de qualquer chamada ao motor.</summary>
public sealed class ExecuteCreditInquiryValidator : AbstractValidator<ExecuteCreditInquiryRequest>
{
    public ExecuteCreditInquiryValidator()
    {
        RuleFor(request => request.BrokerageId)
            .NotEmpty().WithMessage("O identificador da corretora é obrigatório.");

        RuleFor(request => request.PolicyHolderCnpj)
            .NotEmpty().WithMessage("O CNPJ do tomador é obrigatório.")
            .Must(CnpjValidator.IsValid).WithMessage("O CNPJ do tomador é inválido.");
    }
}
