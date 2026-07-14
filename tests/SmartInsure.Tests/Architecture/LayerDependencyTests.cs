using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace SmartInsure.Tests.Architecture;

/// <summary>
/// Gate permanente das regras de dependência entre camadas (ADR-052):
/// violação aqui é falha de build, nunca Skip.
/// </summary>
public class LayerDependencyTests
{
    private static readonly Assembly CoreAssembly =
        typeof(global::SmartInsure.Core.Entities.EntityBase).Assembly;

    private static readonly Assembly ApplicationAssembly =
        typeof(global::SmartInsure.Application.UseCase.Common.IUseCase<,>).Assembly;

    private static readonly Assembly InfraDataAssembly =
        typeof(global::SmartInsure.Infra.Data.Context.SmartInsureDbContext).Assembly;

    private static readonly Assembly CrossCuttingAssembly =
        typeof(global::SmartInsure.Infra.CrossCutting.Options.JwtOptions).Assembly;

    private static readonly Assembly BackgroundServicesAssembly =
        typeof(global::SmartInsure.Infra.BackgroundServices.DependencyInjection).Assembly;

    [Fact]
    public void Core_NaoDeveDepender_DeNenhumaOutraCamadaOuPacoteExterno()
    {
        // ADR-026: o Core é a camada mais interna, sem dependências externas.
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SmartInsure.Api",
                "SmartInsure.Application",
                "SmartInsure.Infra",
                "SmartInsure.Integration",
                "SmartInsure.MailServices",
                "SmartInsure.Functions",
                "Microsoft.EntityFrameworkCore",
                "MongoDB",
                "FluentValidation",
                "Carter",
                "Refit",
                "MailKit")
            .GetResult();

        AssertSuccess(result);
    }

    [Fact]
    public void Application_NaoDeveDepender_DaInfraestruturaDeDadosNemDaApi()
    {
        // ADR-023: acesso a dados só via contratos do Core (repositórios + IUnitOfWork).
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SmartInsure.Api",
                "SmartInsure.Infra.Data",
                "SmartInsure.Infra.BackgroundServices",
                "Microsoft.EntityFrameworkCore",
                "MongoDB",
                "Carter")
            .GetResult();

        AssertSuccess(result);
    }

    [Fact]
    public void InfraData_NaoDeveDepender_DeCamadasSuperiores()
    {
        var result = Types.InAssembly(InfraDataAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SmartInsure.Api",
                "SmartInsure.Application",
                "SmartInsure.Infra.CrossCutting",
                "SmartInsure.Infra.BackgroundServices",
                "SmartInsure.Integration",
                "SmartInsure.MailServices")
            .GetResult();

        AssertSuccess(result);
    }

    [Fact]
    public void InfraCrossCutting_NaoDeveDepender_DeCamadasSuperiores()
    {
        var result = Types.InAssembly(CrossCuttingAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SmartInsure.Api",
                "SmartInsure.Application",
                "SmartInsure.Infra.Data",
                "SmartInsure.Infra.BackgroundServices",
                "SmartInsure.Integration",
                "SmartInsure.MailServices")
            .GetResult();

        AssertSuccess(result);
    }

    [Fact]
    public void InfraBackgroundServices_NaoDeveDepender_DeCamadasSuperiores()
    {
        var result = Types.InAssembly(BackgroundServicesAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SmartInsure.Api",
                "SmartInsure.Application",
                "SmartInsure.Infra.Data",
                "SmartInsure.Infra.CrossCutting",
                "SmartInsure.Integration",
                "SmartInsure.MailServices")
            .GetResult();

        AssertSuccess(result);
    }

    private static void AssertSuccess(TestResult result)
        => result.IsSuccessful.Should().BeTrue(
            $"tipos violando a regra de dependência: {string.Join(", ", result.FailingTypeNames ?? [])}");
}
