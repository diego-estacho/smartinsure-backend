using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator.Interfaces;

/// <summary>Contrato do convite de Corretor Administrador (RN-036).</summary>
public interface IInviteBrokerageAdministratorUseCase
    : IUseCase<InviteBrokerageAdministratorRequest, InviteBrokerageAdministratorResponse>;
