using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer;

/// <summary>RN-005 — Criação de Seguradora no catálogo: CNPJ único, dados manuais (sem Birô).</summary>
public sealed class CreateInsurerUseCase(
    IInsurerRepository insurerRepository,
    IUnitOfWork unitOfWork) : ICreateInsurerUseCase
{
    public async Task<CreateInsurerResponse> ExecuteAsync(
        CreateInsurerRequest request,
        CancellationToken cancellationToken)
    {
        var cnpj = CnpjValidator.Normalize(request.Cnpj);

        if (await insurerRepository.CnpjExistsAsync(cnpj, null, cancellationToken))
        {
            throw new ConflictException("Já existe uma seguradora com este CNPJ no catálogo.");
        }

        if (!Enum.TryParse<EInsurerStatus>(request.InitialStatus, ignoreCase: true, out var initialStatus))
        {
            throw new BusinessRuleException("A situação inicial deve ser Active ou Inactive.");
        }
        var insurer = Insurer.Create(
            cnpj, request.CorporateName, request.TradeName, request.LogoUrl, initialStatus);

        await insurerRepository.AddAsync(insurer, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new CreateInsurerResponse(
            insurer.Id,
            insurer.Cnpj,
            insurer.CorporateName,
            insurer.TradeName,
            insurer.LogoUrl,
            insurer.Status.ToString());
    }
}
