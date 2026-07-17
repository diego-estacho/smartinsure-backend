using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Responses;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Interfaces;

public interface ICreateBrokerageUseCase : IUseCase<CreateBrokerageRequest, CreateBrokerageResponse>;
