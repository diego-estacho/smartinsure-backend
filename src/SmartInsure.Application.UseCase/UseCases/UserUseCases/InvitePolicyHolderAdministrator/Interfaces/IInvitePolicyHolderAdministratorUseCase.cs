using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderAdministrator.Interfaces;

/// <summary>Contrato do convite de Tomador Administrador (RN-068).</summary>
public interface IInvitePolicyHolderAdministratorUseCase
    : IUseCase<InvitePolicyHolderAdministratorRequest, InvitePolicyHolderAdministratorResponse>;
