using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Responses;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Interfaces;

public interface IPreviewBrokerageByCnpjUseCase
    : IUseCase<PreviewBrokerageByCnpjRequest, BrokeragePreviewResponse>;
