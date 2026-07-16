using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer;

/// <summary>RN-008 — Alteração cadastral mantém CNPJ único/válido e razão social; situação intacta.</summary>
public sealed class UpdateInsurerUseCase(
    IInsurerRepository insurerRepository,
    IUnitOfWork unitOfWork) : IUpdateInsurerUseCase
{
    public async Task<UpdateInsurerResponse> ExecuteAsync(
        UpdateInsurerRequest request,
        CancellationToken cancellationToken)
    {
        var insurer = await insurerRepository.GetByIdAsync(request.InsurerId, cancellationToken)
            ?? throw new NotFoundException("Seguradora não encontrada no catálogo.");

        var cnpj = CnpjValidator.Normalize(request.Cnpj);

        if (await insurerRepository.CnpjExistsAsync(cnpj, insurer.Id, cancellationToken))
        {
            throw new ConflictException("Já existe uma seguradora com este CNPJ no catálogo.");
        }

        insurer.UpdateDetails(cnpj, request.CorporateName, request.TradeName, request.LogoUrl);
        await unitOfWork.CommitAsync(cancellationToken);

        return new UpdateInsurerResponse(
            insurer.Id,
            insurer.Cnpj,
            insurer.CorporateName,
            insurer.TradeName,
            insurer.LogoUrl,
            insurer.Status.ToString());
    }
}
