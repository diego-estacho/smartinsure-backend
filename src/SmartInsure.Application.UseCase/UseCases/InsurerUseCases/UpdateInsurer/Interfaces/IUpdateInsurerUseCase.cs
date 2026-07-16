using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Responses;

namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Interfaces;

/// <summary>Contrato para alteração cadastral de Seguradora (RN-006).</summary>
public interface IUpdateInsurerUseCase : IUseCase<UpdateInsurerRequest, UpdateInsurerResponse>;
