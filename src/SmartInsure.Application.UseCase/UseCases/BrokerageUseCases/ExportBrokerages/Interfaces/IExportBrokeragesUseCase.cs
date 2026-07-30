using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ExportBrokerages.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ExportBrokerages.Responses;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ExportBrokerages.Interfaces;

public interface IExportBrokeragesUseCase
    : IUseCase<ExportBrokeragesRequest, ExportBrokeragesResponse>;
