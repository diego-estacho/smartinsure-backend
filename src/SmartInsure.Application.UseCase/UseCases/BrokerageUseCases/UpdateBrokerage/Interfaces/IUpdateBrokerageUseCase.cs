using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Responses;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.UpdateBrokerage.Requests;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.UpdateBrokerage.Interfaces;

public interface IUpdateBrokerageUseCase
    : IUseCase<UpdateBrokerageRequest, GetBrokerageResponse>;
