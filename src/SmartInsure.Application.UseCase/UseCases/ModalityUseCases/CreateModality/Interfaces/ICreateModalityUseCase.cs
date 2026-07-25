using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Responses;

namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Interfaces;

public interface ICreateModalityUseCase : IUseCase<CreateModalityRequest, CreateModalityResponse>;
