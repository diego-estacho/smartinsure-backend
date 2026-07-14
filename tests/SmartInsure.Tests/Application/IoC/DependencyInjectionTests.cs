using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Application.UseCase.IoC;

namespace SmartInsure.Tests.Application.IoC;

/// <summary>Cobertura da convenção de scanning exigida pela ADR-021.</summary>
public class DependencyInjectionTests
{
    [Fact]
    public void AddApplicationUseCases_DeveRegistrarComoScoped_TodoUseCaseQueSegueAConvencao()
    {
        var services = new ServiceCollection();
        services.AddApplicationUseCases();

        var expected = typeof(DependencyInjection).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && type.Name.EndsWith("UseCase", StringComparison.Ordinal))
            .Select(implementation => (
                Contract: implementation.GetInterfaces()
                    .FirstOrDefault(candidate => candidate.Name == $"I{implementation.Name}"),
                Implementation: implementation))
            .Where(pair => pair.Contract is not null)
            .ToList();

        foreach (var (contract, implementation) in expected)
        {
            services.Should().ContainSingle(descriptor =>
                descriptor.ServiceType == contract
                && descriptor.ImplementationType == implementation
                && descriptor.Lifetime == ServiceLifetime.Scoped);
        }
    }
}
