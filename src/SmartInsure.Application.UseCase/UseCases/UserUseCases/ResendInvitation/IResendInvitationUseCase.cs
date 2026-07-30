using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Interfaces;

public interface IResendInvitationUseCase : IUseCase<ResendInvitationRequest, ResendInvitationResponse>
{
}
