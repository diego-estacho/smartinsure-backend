using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Responses;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Interfaces;

public interface IListBrokeragesUseCase
    : IUseCase<ListBrokeragesRequest, PagedResponse<BrokerageListItemResponse>>;
