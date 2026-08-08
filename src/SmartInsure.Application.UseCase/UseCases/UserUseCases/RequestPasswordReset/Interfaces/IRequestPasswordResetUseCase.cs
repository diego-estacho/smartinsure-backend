using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.RequestPasswordReset.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.RequestPasswordReset.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.RequestPasswordReset.Interfaces;

public interface IRequestPasswordResetUseCase
    : IUseCase<RequestPasswordResetRequest, RequestPasswordResetResponse>
{
}
