using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerageHistory.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerageHistory.Responses;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerageHistory.Interfaces;

public interface IGetBrokerageHistoryUseCase
    : IUseCase<GetBrokerageHistoryRequest, GetBrokerageHistoryResponse>;
