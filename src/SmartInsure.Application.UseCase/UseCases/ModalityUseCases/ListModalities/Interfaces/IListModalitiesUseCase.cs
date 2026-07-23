using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ListModalities.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ListModalities.Responses;

namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ListModalities.Interfaces;

public interface IListModalitiesUseCase : IUseCase<ListModalitiesRequest, PagedResponse<ModalityListItemResponse>>;
