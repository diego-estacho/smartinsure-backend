using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Responses;

namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Interfaces;

public interface IChangeModalityStatusUseCase : IUseCase<ChangeModalityStatusRequest, ChangeModalityStatusResponse>;
