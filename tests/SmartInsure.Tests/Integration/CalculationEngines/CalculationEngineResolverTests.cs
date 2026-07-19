using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Integration.CalculationEngines.Services;

namespace SmartInsure.Tests.Integration.CalculationEngines;

/// <summary>RN-023 — Seleção do Motor de Cálculo pela Habilitação.</summary>
[Trait("RuleId", "RN-023")]
public class CalculationEngineResolverTests
{
    private static readonly Guid BrokerageId = Guid.CreateVersion7();
    private static readonly Guid InsurerId = Guid.CreateVersion7();

    private readonly IInsurerEnablementRepository _enablementRepository =
        Substitute.For<IInsurerEnablementRepository>();

    private readonly IInsurerRepository _insurerRepository = Substitute.For<IInsurerRepository>();

    private static Insurer ActiveInsurer()
        => Insurer.Create("98765432000109", "Seguradora Beta S.A.", null, null, EInsurerStatus.Active);

    private static InsurerEnablement PlugV2Enablement(string? connectionParameters = null)
        => InsurerEnablement.Create(
            BrokerageId, InsurerId, ECalculationEngine.PlugV2, connectionParameters);

    private CalculationEngineResolver BuildResolver(bool registerPlugV2 = true)
    {
        var services = new ServiceCollection();

        if (registerPlugV2)
        {
            services.AddKeyedScoped<ICalculationEngine, PlugV2CalculationEngine>(
                ECalculationEngine.PlugV2);
        }

        return new CalculationEngineResolver(
            _enablementRepository, _insurerRepository, services.BuildServiceProvider());
    }

    [Fact]
    public async Task Resolve_DeveDevolverMotorComParametrosDoVinculo_QuandoHabilitacaoAtiva()
    {
        _insurerRepository.GetByIdAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns(ActiveInsurer());
        _enablementRepository.GetByPairAsync(BrokerageId, InsurerId, Arg.Any<CancellationToken>())
            .Returns(PlugV2Enablement("""{"brokerCnpj":"12345678000195"}"""));

        var resolution = await BuildResolver()
            .ResolveAsync(BrokerageId, InsurerId, CancellationToken.None);

        resolution.Engine.Engine.Should().Be(ECalculationEngine.PlugV2);
        resolution.ConnectionParameters.Should().Be("""{"brokerCnpj":"12345678000195"}""");
    }

    [Fact]
    public async Task Resolve_DeveRecusar_QuandoParSemHabilitacao()
    {
        _insurerRepository.GetByIdAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns(ActiveInsurer());
        _enablementRepository.GetByPairAsync(BrokerageId, InsurerId, Arg.Any<CancellationToken>())
            .Returns((InsurerEnablement?)null);

        var act = () => BuildResolver().ResolveAsync(BrokerageId, InsurerId, CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessRuleException>())
            .WithMessage("*não está habilitada*");
    }

    [Fact]
    public async Task Resolve_DeveRecusar_QuandoHabilitacaoInativa()
    {
        var enablement = PlugV2Enablement();
        enablement.Deactivate();
        _insurerRepository.GetByIdAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns(ActiveInsurer());
        _enablementRepository.GetByPairAsync(BrokerageId, InsurerId, Arg.Any<CancellationToken>())
            .Returns(enablement);

        var act = () => BuildResolver().ResolveAsync(BrokerageId, InsurerId, CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessRuleException>())
            .WithMessage("*não está habilitada*");
    }

    [Fact]
    public async Task Resolve_DeveRecusar_QuandoSeguradoraInativaNoCatalogo()
    {
        var insurer = ActiveInsurer();
        insurer.Deactivate();
        _insurerRepository.GetByIdAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns(insurer);

        var act = () => BuildResolver().ResolveAsync(BrokerageId, InsurerId, CancellationToken.None);

        // RN-010: Seguradora Inativa fica fora dos fluxos operacionais, mesmo com Habilitação Ativa.
        await act.Should().ThrowAsync<BusinessRuleException>();
        await _enablementRepository.DidNotReceiveWithAnyArgs()
            .GetByPairAsync(default, default, default);
    }

    [Fact]
    public async Task Resolve_DeveRecusar_QuandoSeguradoraNaoEncontrada()
    {
        _insurerRepository.GetByIdAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns((Insurer?)null);

        var act = () => BuildResolver().ResolveAsync(BrokerageId, InsurerId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Resolve_DeveRecusar_QuandoMotorNaoDisponivelNaPlataforma()
    {
        _insurerRepository.GetByIdAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns(ActiveInsurer());
        _enablementRepository.GetByPairAsync(BrokerageId, InsurerId, Arg.Any<CancellationToken>())
            .Returns(PlugV2Enablement());

        var act = () => BuildResolver(registerPlugV2: false)
            .ResolveAsync(BrokerageId, InsurerId, CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessRuleException>())
            .WithMessage("*motor de cálculo*");
    }
}
