using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.GetModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.GetModality.Responses;

namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.GetModality.Interfaces;

public interface IGetModalityUseCase : IUseCase<GetModalityRequest, GetModalityResponse>;
