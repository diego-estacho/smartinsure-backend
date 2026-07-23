using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Application.UseCase.Services.ModalityImports;
using SmartInsure.Application.UseCase.Services.PersonImports;

namespace SmartInsure.Application.UseCase.IoC;

public static class DependencyInjection
{
    /// <summary>
    /// Registro por assembly scanning (ADR-021): convenção I{Ação}UseCase → {Ação}UseCase
    /// com lifetime Scoped; validators via AddValidatorsFromAssembly. Registro manual
    /// um-a-um nunca é feito — serviço fora da convenção é registrado explicitamente
    /// ao lado, com comentário do porquê.
    /// </summary>
    public static IServiceCollection AddApplicationUseCases(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        var useCaseImplementations = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && type.Name.EndsWith("UseCase", StringComparison.Ordinal));

        foreach (var implementation in useCaseImplementations)
        {
            var contract = implementation.GetInterfaces()
                .FirstOrDefault(candidate => candidate.Name == $"I{implementation.Name}");

            if (contract is not null)
            {
                services.AddScoped(contract, implementation);
            }
        }

        services.AddValidatorsFromAssembly(assembly);

        // Serviço compartilhado por use cases; fora da convenção I{Ação}UseCase → {Ação}UseCase.
        services.AddScoped<IPersonBureauImporter, PersonBureauImporter>();

        // Serviço de importação de modalidades (RN-034), orquestrado pelo timer das Functions.
        services.AddScoped<IModalityImporter, ModalityImporter>();

        return services;
    }
}
