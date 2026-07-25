using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerageHistory.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerageHistory.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerageHistory.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerageHistory;

/// <summary>
/// RN-035 — linha do tempo da Corretora derivada da auditoria. Uma Corretora existente tem sempre ao
/// menos o evento de criação; timeline vazia significa Corretora inexistente (404).
/// </summary>
public sealed class GetBrokerageHistoryUseCase(IPersonRepository personRepository)
    : IGetBrokerageHistoryUseCase
{
    public async Task<GetBrokerageHistoryResponse> ExecuteAsync(
        GetBrokerageHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var events = await personRepository.GetBrokerageHistoryAsync(request.BrokerageId, cancellationToken);

        if (events.Count == 0)
        {
            throw new NotFoundException("Corretora não encontrada.");
        }

        return new GetBrokerageHistoryResponse(
            [.. events.Select(historyEvent => new BrokerageHistoryEventResponse(
                historyEvent.Type,
                historyEvent.Subject,
                historyEvent.OccurredAt,
                historyEvent.Author))]);
    }
}
