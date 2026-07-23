using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AcceptInvitation.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.AcceptInvitation;

/// <summary>RN-035 — aceite do convite: define senha + ativa Usuário (RN-002: Pendente→Ativo).</summary>
[Trait("RuleId", "RN-035")]
[Trait("RuleId", "RN-002")]
public class AcceptInvitationUseCaseTests
{
    private readonly IInvitationRepository _invitationRepository = Substitute.For<IInvitationRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IIdentityProvider _identityProvider = Substitute.For<IIdentityProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AcceptInvitationUseCase _useCase;

    public AcceptInvitationUseCaseTests()
    {
        _useCase = new AcceptInvitationUseCase(
            _invitationRepository, _userRepository, _identityProvider, _unitOfWork,
            Substitute.For<Microsoft.Extensions.Logging.ILogger<AcceptInvitationUseCase>>());
    }

    [Fact]
    public async Task Execute_DeveAtivuser_QuandoConviteValido()
    {
        var user = User.Create("João", "joao@example.com", "casdoor-123");
        var (invitation, plainToken) = Invitation.Create(user.Id, 7);

        _invitationRepository.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(invitation);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var response = await _useCase.ExecuteAsync(
            new AcceptInvitationRequest(plainToken, "senhaForte123"), CancellationToken.None);

        response.UserId.Should().Be(user.Id);
        response.Email.Should().Be(user.Email);
        response.Status.Should().Be("Active"); // RN-002: Pendente → Ativo no primeiro acesso
        user.Status.Should().Be(SmartInsure.Core.Enumerators.EUserStatus.Active);
        invitation.IsValid().Should().BeFalse(); // Deve estar consumido
        await _identityProvider.Received(1)
            .SetPasswordAsync("casdoor-123", "senhaForte123", Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoConviteExpirado()
    {
        var (invitation, plainToken) = Invitation.Create(Guid.CreateVersion7(), -1); // Expirado

        _invitationRepository.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(invitation);

        var act = () => _useCase.ExecuteAsync(
            new AcceptInvitationRequest(plainToken, "senha123"), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoTokenInvalido()
    {
        _invitationRepository.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Invitation?)null);

        var act = () => _useCase.ExecuteAsync(
            new AcceptInvitationRequest("tokenInvalido", "senha123"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
