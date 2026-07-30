using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Interfaces;

/// <summary>Contrato da criação de Usuário no Tomador ativo (RN-070).</summary>
public interface IInvitePolicyHolderUserUseCase
    : IUseCase<InvitePolicyHolderUserRequest, InvitePolicyHolderUserResponse>;
