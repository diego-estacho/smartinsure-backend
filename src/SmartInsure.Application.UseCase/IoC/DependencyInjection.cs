using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Application.UseCase.Services.AdditionalCoverageImports;
using SmartInsure.Application.UseCase.Services.Invitations;
using SmartInsure.Application.UseCase.Services.ModalityImports;
using SmartInsure.Application.UseCase.Services.PersonImports;
using SmartInsure.Application.UseCase.Services.Quotations;
using SmartInsure.Application.UseCase.Services.Scopes;
using SmartInsure.Core.Abstractions;

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

        // Serviços compartilhados por use cases; fora da convenção I{Ação}UseCase → {Ação}UseCase.
        services.AddScoped<IPersonBureauImporter, PersonBureauImporter>();
        services.AddScoped<IInvitationMailer, InvitationMailer>();

        // RN-064/ADR-065: resolução do Escopo ativo, compartilhada pelo login e pela troca de Escopo.
        services.AddScoped<IActiveScopeResolver, ActiveScopeResolver>();

        // RN-068/RN-069/RN-070: quem administra o Escopo ativo, e a criação de Usuário convidado
        // compartilhada pelos fluxos de Corretor/Tomador Administrador.
        services.AddScoped<IScopeAuthorization, ScopeAuthorization>();
        services.AddScoped<IInvitedUserService, InvitedUserService>();

        // Serviço de importação de modalidades (RN-034), orquestrado pelo timer das Functions.
        services.AddScoped<IModalityImporter, ModalityImporter>();

        // Serviço de importação de Coberturas Adicionais (RN-044), orquestrado pelo timer e pelo disparo sob demanda.
        services.AddScoped<IAdditionalCoverageImporter, AdditionalCoverageImporter>();

        // Processor do fan-out de cotação (RN-057), resolvido em escopo pelo consumidor (ADR-050);
        // fora da convenção I{Ação}UseCase, por isso registrado explicitamente.
        services.AddScoped<IQuotationRequestProcessor, QuotationRequestProcessor>();

        return services;
    }
}
