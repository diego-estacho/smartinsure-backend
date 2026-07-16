using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.LogoutUser.Requests;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.LogoutUser.Interfaces;

/// <summary>Contrato para o caso de uso de encerramento de sessão.</summary>
public interface ILogoutUserUseCase : IUseCase<LogoutUserRequest, Unit>;
