using Microsoft.Extensions.Options;
using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Options;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ResendInvitation;

/// <summary>RN-035: reenvio do convite — invalida o anterior e envia novo por e-mail.</summary>
public sealed class ResendInvitationUseCase(
    IInvitationRepository invitationRepository,
    IUserRepository userRepository,
    IMailService mailService,
    IUnitOfWork unitOfWork,
    IOptions<InvitationOptions> invitationOptions) : IResendInvitationUseCase
{
    public async Task<ResendInvitationResponse> ExecuteAsync(
        ResendInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Usuário não encontrado.");
        }

        // RN-035: só Pendentes podem receber novo convite (a constraint DB garante um ativo).
        var oldInvitation = await invitationRepository.GetPendingByUserAsync(user.Id, cancellationToken);

        if (oldInvitation is not null)
        {
            // Invalida o anterior e PERSISTE antes de inserir o novo: o índice único filtrado
            // (um Convite ativo por Usuário) proíbe dois ativos, e o EF não garante UPDATE antes de INSERT
            // no mesmo commit — por isso o consume é gravado primeiro.
            oldInvitation.Consume();
            invitationRepository.Update(oldInvitation);
            await unitOfWork.CommitAsync(cancellationToken);
        }

        // RN-035: gera novo convite.
        var (newInvitation, plainToken) = Invitation.Create(user.Id, invitationOptions.Value.LinkExpiryDays);
        await invitationRepository.AddAsync(newInvitation, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        var invitationLink = $"{invitationOptions.Value.AppBaseUrl}/invite?token={Uri.EscapeDataString(plainToken)}";
        var htmlBody = BuildInvitationEmailHtml(user.Name, invitationLink);

        await mailService.SendAsync(
            new MailMessage
            {
                To = [user.Email],
                Subject = "SmartInsure — Novo link de acesso",
                HtmlBody = htmlBody,
            },
            cancellationToken);

        return new ResendInvitationResponse(user.Id, user.Email);
    }

    private static string BuildInvitationEmailHtml(string userName, string invitationLink)
        => $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1>SmartInsure — Novo link de acesso</h1>
        <p>Olá {System.Web.HttpUtility.HtmlEncode(userName)},</p>
        <p>Um novo link para completar seu acesso foi gerado. Clique abaixo:</p>
        <p style='text-align: center; margin: 30px 0;'>
            <a href='{System.Web.HttpUtility.HtmlAttributeEncode(invitationLink)}'
               style='display: inline-block; background-color: #0066cc; color: white; padding: 12px 30px; text-decoration: none; border-radius: 4px;'>
                Completar acesso
            </a>
        </p>
        <p style='color: #666; font-size: 12px;'>
            Este link expira em 7 dias.
        </p>
    </div>
</body>
</html>";
}
