using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ListModalityGroups.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ListModalityGroups.Responses;

namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ListModalityGroups.Interfaces;

public interface IListModalityGroupsUseCase
    : IUseCase<ListModalityGroupsRequest, PagedResponse<ModalityGroupListItemResponse>>;
