using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ListInsurers.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ListInsurers.Responses;

namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ListInsurers.Interfaces;

/// <summary>Consulta paginada do catálogo de Seguradoras (RN-008).</summary>
public interface IListInsurersUseCase : IUseCase<ListInsurersRequest, PagedResponse<InsurerListItemResponse>>
{
}
