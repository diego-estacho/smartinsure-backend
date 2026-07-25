using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Responses;

namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Interfaces;

public interface IUpdateModalityUseCase : IUseCase<UpdateModalityRequest, UpdateModalityResponse>;
