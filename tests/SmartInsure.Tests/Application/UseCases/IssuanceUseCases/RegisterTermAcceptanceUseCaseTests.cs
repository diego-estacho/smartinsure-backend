using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.IssuanceUseCases;

/// <summary>
/// RN-506 — Termo e declaração: emitir exige aceite explícito do Termo vigente da Seguradora, e a
/// plataforma registra quem aceitou, quando, o conteúdo exato aceito e o agente de acesso. Sem esse
/// conteúdo não há como provar O QUE foi aceito.
/// </summary>
[Trait("RuleId", "RN-506")]
public class RegisterTermAcceptanceUseCaseTests
{
    private readonly IInsurerTermRepository _termRepository = Substitute.For<IInsurerTermRepository>();
    private readonly ITermAcceptanceRepository _acceptanceRepository = Substitute.For<ITermAcceptanceRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly Guid _insurerId = Guid.CreateVersion7();
    private const string TermContent = "O tomador declara que os dados informados são verdadeiros.";
    private const string ExternalIdentity = "casdoor|diegoteste01";

    private User _user = null!;

    private RegisterTermAcceptanceUseCase BuildUseCase(bool withActiveTerm = true)
    {
        _user = User.Create("Diego", "diego@onpoint.com.br", ExternalIdentity);

        _currentUser.UserIdentifier.Returns(ExternalIdentity);
        _userRepository.GetByExternalIdentityAsync(ExternalIdentity, Arg.Any<CancellationToken>()).Returns(_user);

        if (withActiveTerm)
        {
            _termRepository.GetActiveByInsurerAsync(_insurerId, Arg.Any<CancellationToken>())
                .Returns(InsurerTerm.Create(_insurerId, TermContent));
        }

        return new RegisterTermAcceptanceUseCase(
            _termRepository, _acceptanceRepository, _userRepository, _currentUser, _unitOfWork);
    }

    private RegisterTermAcceptanceRequest Request()
        => new() { InsurerId = _insurerId, UserAgent = "Mozilla/5.0 (Windows NT 10.0)" };

    [Fact]
    public async Task Execute_DeveRegistrarUsuarioInstanteConteudoEAgenteDoAceite()
    {
        var useCase = BuildUseCase();
        TermAcceptance? persisted = null;
        await _acceptanceRepository.AddAsync(
            Arg.Do<TermAcceptance>(acceptance => persisted = acceptance), Arg.Any<CancellationToken>());

        var response = await useCase.ExecuteAsync(Request(), CancellationToken.None);

        persisted.Should().NotBeNull();
        persisted!.UserId.Should().Be(_user.Id);
        persisted.AcceptedContent.Should().Be(TermContent, "a prova é o texto exibido, não um ponteiro para ele");
        persisted.UserAgent.Should().Be("Mozilla/5.0 (Windows NT 10.0)");
        persisted.AcceptedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        response.TermAcceptanceId.Should().Be(persisted.Id);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_SeguradoraSemTermoVigente_DeveRecusar()
    {
        var useCase = BuildUseCase(withActiveTerm: false);

        var act = () => useCase.ExecuteAsync(Request(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*Termo*");
        await _acceptanceRepository.DidNotReceive().AddAsync(Arg.Any<TermAcceptance>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DuasVezes_DeveRegistrarCadaAceite()
    {
        var useCase = BuildUseCase();

        await useCase.ExecuteAsync(Request(), CancellationToken.None);
        await useCase.ExecuteAsync(Request(), CancellationToken.None);

        await _acceptanceRepository.Received(2).AddAsync(Arg.Any<TermAcceptance>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_SemUsuarioAutenticado_DeveRecusar()
    {
        var useCase = BuildUseCase();
        _currentUser.UserIdentifier.Returns((string?)null);

        var act = () => useCase.ExecuteAsync(Request(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _acceptanceRepository.DidNotReceive().AddAsync(Arg.Any<TermAcceptance>(), Arg.Any<CancellationToken>());
    }
}
