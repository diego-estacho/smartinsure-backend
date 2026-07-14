using System.Reflection;
using Carter;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartInsure.Application.UseCase.Common;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;

namespace SmartInsure.Tests.Architecture;

/// <summary>
/// Convenções estruturais travadas como gate (ADR-052): herança da base de auditoria
/// (ADR-030), enums com prefixo E (ADR-031) e sufixos de naming (Endpoint/Repository/
/// UseCase/Mapping).
/// </summary>
public class ConventionTests
{
    private static readonly Assembly CoreAssembly =
        typeof(EntityBase).Assembly;

    private static readonly Assembly ApplicationAssembly =
        typeof(IUseCase<,>).Assembly;

    private static readonly Assembly InfraDataAssembly =
        typeof(global::SmartInsure.Infra.Data.Context.SmartInsureDbContext).Assembly;

    private static readonly Assembly ApiAssembly =
        typeof(global::SmartInsure.Api.Handlers.Base.RequestHandler).Assembly;

    [Fact]
    public void Entidades_DevemHerdarEntityBase_QuandoDeclaradasEmCoreEntities()
    {
        var offenders = CoreAssembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && type.Namespace?.StartsWith("SmartInsure.Core.Entities", StringComparison.Ordinal) == true
                && !typeof(EntityBase).IsAssignableFrom(type));

        offenders.Should().BeEmpty("toda entidade deve herdar EntityBase (ADR-030)");
    }

    [Fact]
    public void EnumsDeDominio_DevemTerPrefixoEEViverEmEnumerators_QuandoDeclaradosNoCore()
    {
        var offenders = CoreAssembly.GetTypes()
            .Where(type => type.IsEnum
                && (type.Namespace?.StartsWith("SmartInsure.Core.Enumerators", StringComparison.Ordinal) != true
                    || !type.Name.StartsWith('E')));

        offenders.Should().BeEmpty("enums de domínio vivem em Core/Enumerators com prefixo E (ADR-031)");
    }

    [Fact]
    public void CarterModules_DevemTerSufixoEndpoint_QuandoDeclaradosNaApi()
    {
        var offenders = ApiAssembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && typeof(ICarterModule).IsAssignableFrom(type)
                && !type.Name.EndsWith("Endpoint", StringComparison.Ordinal));

        offenders.Should().BeEmpty("todo Carter module segue {Entidade}Endpoint (ADR-009)");
    }

    [Fact]
    public void Repositorios_DevemTerSufixoRepository_QuandoImplementamIRepository()
    {
        var offenders = InfraDataAssembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && type.GetInterfaces().Any(candidate => candidate.IsGenericType
                    && candidate.GetGenericTypeDefinition() == typeof(IRepository<>))
                && !type.Name.EndsWith("Repository", StringComparison.Ordinal));

        offenders.Should().BeEmpty("todo repositório segue {Entidade}Repository (ADR-036)");
    }

    [Fact]
    public void Mappings_DevemTerSufixoMapping_QuandoImplementamIEntityTypeConfiguration()
    {
        var offenders = InfraDataAssembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && type.GetInterfaces().Any(candidate => candidate.IsGenericType
                    && candidate.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                && !type.Name.EndsWith("Mapping", StringComparison.Ordinal));

        offenders.Should().BeEmpty("todo mapping segue {Entidade}Mapping (ADR-033)");
    }

    [Fact]
    public void UseCases_DevemTerSufixoUseCase_QuandoImplementamIUseCase()
    {
        var offenders = ApplicationAssembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && type.GetInterfaces().Any(candidate => candidate.IsGenericType
                    && candidate.GetGenericTypeDefinition() == typeof(IUseCase<,>))
                && !type.Name.EndsWith("UseCase", StringComparison.Ordinal));

        offenders.Should().BeEmpty("todo UseCase segue {Ação}UseCase (ADR-020)");
    }
}
