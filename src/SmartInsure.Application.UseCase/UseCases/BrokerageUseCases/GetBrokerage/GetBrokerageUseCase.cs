using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage;

/// <summary>RN-020 — detalhes de Corretora a partir da Pessoa jurídica com papel Corretor.</summary>
public sealed class GetBrokerageUseCase(IPersonRepository personRepository) : IGetBrokerageUseCase
{
    public async Task<GetBrokerageResponse> ExecuteAsync(
        GetBrokerageRequest request,
        CancellationToken cancellationToken)
    {
        var brokerage = await personRepository.GetBrokerageByIdAsync(
            request.BrokerageId, cancellationToken)
            ?? throw new NotFoundException("Corretora não encontrada.");

        return new GetBrokerageResponse(
            brokerage.Id,
            brokerage.DocumentNumber,
            brokerage.Name,
            brokerage.SocialName,
            brokerage.LegalNatureCode,
            brokerage.LegalNatureDescription,
            brokerage.IsPrivateSector,
            brokerage.Status,
            brokerage.MainAddress is null
                ? null
                : new BrokerageAddressResponse(
                    brokerage.MainAddress.ZipCode,
                    brokerage.MainAddress.Street,
                    brokerage.MainAddress.Number,
                    brokerage.MainAddress.Complement,
                    brokerage.MainAddress.Neighborhood,
                    brokerage.MainAddress.City,
                    brokerage.MainAddress.State));
    }
}
