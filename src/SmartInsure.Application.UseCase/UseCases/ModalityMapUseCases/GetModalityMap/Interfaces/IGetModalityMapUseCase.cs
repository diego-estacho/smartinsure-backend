using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Responses;

namespace SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Interfaces;

public interface IGetModalityMapUseCase : IUseCase<GetModalityMapRequest, ModalityMapResponse>;
