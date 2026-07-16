using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.GetInsurer.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.GetInsurer.Responses;

namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.GetInsurer.Interfaces;

/// <summary>Consulta de detalhe de Seguradora do catálogo (RN-008).</summary>
public interface IGetInsurerUseCase : IUseCase<GetInsurerRequest, GetInsurerResponse>
{
}
