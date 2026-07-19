using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ListInsurerEnablements.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ListInsurerEnablements.Responses;

namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ListInsurerEnablements.Interfaces;

public interface IListInsurerEnablementsUseCase
    : IUseCase<ListInsurerEnablementsRequest, PagedResponse<InsurerEnablementListItemResponse>>;
