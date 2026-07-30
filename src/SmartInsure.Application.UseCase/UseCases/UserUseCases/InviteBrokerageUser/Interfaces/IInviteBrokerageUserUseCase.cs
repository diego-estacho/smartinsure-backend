using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Interfaces;

/// <summary>Contrato da criação de Usuário na Corretora ativa (RN-069).</summary>
public interface IInviteBrokerageUserUseCase
    : IUseCase<InviteBrokerageUserRequest, InviteBrokerageUserResponse>;
