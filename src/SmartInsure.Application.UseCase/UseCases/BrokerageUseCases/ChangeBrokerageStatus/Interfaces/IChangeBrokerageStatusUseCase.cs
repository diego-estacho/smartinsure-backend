using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Responses;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Interfaces;

public interface IChangeBrokerageStatusUseCase
    : IUseCase<ChangeBrokerageStatusRequest, ChangeBrokerageStatusResponse>;
