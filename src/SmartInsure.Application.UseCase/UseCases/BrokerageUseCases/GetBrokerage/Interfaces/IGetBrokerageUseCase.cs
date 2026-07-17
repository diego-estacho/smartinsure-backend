using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Responses;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Interfaces;

public interface IGetBrokerageUseCase : IUseCase<GetBrokerageRequest, GetBrokerageResponse>;
