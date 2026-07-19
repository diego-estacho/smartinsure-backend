using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.GetInsurerEnablement.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.GetInsurerEnablement.Responses;

namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.GetInsurerEnablement.Interfaces;

public interface IGetInsurerEnablementUseCase
    : IUseCase<GetInsurerEnablementRequest, GetInsurerEnablementResponse>;
