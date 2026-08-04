using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance.Interfaces;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance.Requests;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance;

/// <summary>
/// RN-506 — registra o aceite do Termo e declaração da Seguradora: quem aceitou, quando, o conteúdo
/// exato exibido e o agente de acesso. O texto é copiado no ato porque é a prova do que foi aceito —
/// mudança posterior do Termo vigente não reescreve aceite já dado. Seguradora sem Termo vigente não é
/// emitível, e o bloqueio acontece aqui, antes de qualquer chamada à Seguradora.
/// </summary>
public sealed class RegisterTermAcceptanceUseCase(
    IInsurerTermRepository insurerTermRepository,
    ITermAcceptanceRepository termAcceptanceRepository,
    IUserRepository userRepository,
    ICurrentUserAccessor currentUserAccessor,
    IUnitOfWork unitOfWork) : IRegisterTermAcceptanceUseCase
{
    public async Task<RegisterTermAcceptanceResponse> ExecuteAsync(
        RegisterTermAcceptanceRequest request,
        CancellationToken cancellationToken)
    {
        // O aceite é ato de uma pessoa identificada: execução de sistema não aceita Termo por ninguém.
        var externalIdentity = currentUserAccessor.UserIdentifier;

        if (string.IsNullOrWhiteSpace(externalIdentity))
        {
            throw new BusinessRuleException("O aceite do Termo exige um Usuário autenticado.");
        }

        var user = await userRepository.GetByExternalIdentityAsync(externalIdentity, cancellationToken)
            ?? throw new BusinessRuleException("Usuário autenticado não encontrado na plataforma.");

        var term = await insurerTermRepository.GetActiveByInsurerAsync(request.InsurerId, cancellationToken)
            ?? throw new BusinessRuleException(
                "A Seguradora não tem Termo e declaração vigente cadastrado — emissão indisponível.");

        var acceptance = TermAcceptance.Register(term, user.Id, request.UserAgent, DateTime.UtcNow);

        await termAcceptanceRepository.AddAsync(acceptance, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new RegisterTermAcceptanceResponse
        {
            TermAcceptanceId = acceptance.Id,
            AcceptedAt = acceptance.AcceptedAt,
        };
    }
}
