using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.GetInsurer.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.GetInsurer.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.GetInsurer.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.GetInsurer;

/// <summary>Detalhe de Seguradora do catálogo (leitura, RN-008).</summary>
public sealed class GetInsurerUseCase(IInsurerRepository insurerRepository) : IGetInsurerUseCase
{
    public async Task<GetInsurerResponse> ExecuteAsync(
        GetInsurerRequest request,
        CancellationToken cancellationToken)
    {
        var insurer = await insurerRepository.GetByIdAsync(request.InsurerId, cancellationToken)
            ?? throw new NotFoundException("Seguradora não encontrada no catálogo.");

        return new GetInsurerResponse(
            insurer.Id,
            insurer.Cnpj,
            insurer.CorporateName,
            insurer.TradeName,
            insurer.LogoUrl,
            insurer.Status.ToString());
    }
}
