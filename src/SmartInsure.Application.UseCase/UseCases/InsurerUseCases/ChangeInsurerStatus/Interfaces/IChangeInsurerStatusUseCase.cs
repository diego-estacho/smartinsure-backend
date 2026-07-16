using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Responses;

namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Interfaces;

public interface IChangeInsurerStatusUseCase : IUseCase<ChangeInsurerStatusRequest, ChangeInsurerStatusResponse>;
