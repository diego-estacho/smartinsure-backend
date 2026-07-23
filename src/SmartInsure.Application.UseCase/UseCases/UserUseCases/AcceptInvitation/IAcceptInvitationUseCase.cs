using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Interfaces;

public interface IAcceptInvitationUseCase : IUseCase<AcceptInvitationRequest, AcceptInvitationResponse>
{
}
