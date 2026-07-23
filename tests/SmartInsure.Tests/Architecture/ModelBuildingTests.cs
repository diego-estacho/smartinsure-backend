using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Tests.Architecture;

/// <summary>
/// Constrói o modelo EF (dispara OnModelCreating sem abrir conexão) para validar que os mappings
/// Fluent API são coerentes — cobre o que os testes de caso de uso (mockados) não alcançam.
/// Guarda o alinhamento das entidades novas de Perfil com as migrations (RN-032/RN-033).
/// </summary>
public class ModelBuildingTests
{
    private static IModel BuildModel()
    {
        var options = new DbContextOptionsBuilder<SmartInsureDbContext>()
            .UseSqlServer("Server=none;Database=none;Trusted_Connection=False;")
            .Options;

        using var context = new SmartInsureDbContext(options);
        return context.Model;
    }

    [Fact]
    public void Model_DeveMapearEntidadesDePerfil()
    {
        var model = BuildModel();

        model.FindEntityType(typeof(Profile)).Should().NotBeNull();
        model.FindEntityType(typeof(Permission)).Should().NotBeNull();
        model.FindEntityType(typeof(ProfilePermission)).Should().NotBeNull();
    }

    [Fact]
    public void Model_DeveLigarUsuarioAoPerfilPorFkOpcional()
    {
        var model = BuildModel();
        var user = model.FindEntityType(typeof(User))!;

        var foreignKey = user.GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Profile));

        foreignKey.Properties.Should().ContainSingle()
            .Which.Name.Should().Be(nameof(User.ProfileId));
        foreignKey.IsRequired.Should().BeFalse();
    }

    [Fact]
    public void Model_ProfilePermission_DeveTerFksParaPerfilEPermissao()
    {
        var model = BuildModel();
        var profilePermission = model.FindEntityType(typeof(ProfilePermission))!;

        var principals = profilePermission.GetForeignKeys()
            .Select(fk => fk.PrincipalEntityType.ClrType)
            .ToList();

        principals.Should().Contain(typeof(Profile));
        principals.Should().Contain(typeof(Permission));
    }

    [Fact]
    public void Model_UserBrokerageMembership_DeveTerFksParaUsuarioCorretoraEPerfil()
    {
        var model = BuildModel();
        var membership = model.FindEntityType(typeof(UserBrokerageMembership))!;

        var principals = membership.GetForeignKeys()
            .Select(fk => fk.PrincipalEntityType.ClrType)
            .ToList();

        principals.Should().Contain(typeof(User));
        principals.Should().Contain(typeof(Person));
        principals.Should().Contain(typeof(Profile));
    }

    [Fact]
    public void Model_UserPolicyHolderMembership_DeveTerFksParaUsuarioTomadorEPerfil()
    {
        var model = BuildModel();
        var membership = model.FindEntityType(typeof(UserPolicyHolderMembership))!;

        var principals = membership.GetForeignKeys()
            .Select(fk => fk.PrincipalEntityType.ClrType)
            .ToList();

        principals.Should().Contain(typeof(User));
        principals.Should().Contain(typeof(Person));
        principals.Should().Contain(typeof(Profile));
    }

    [Fact]
    public void Model_Invitation_DeveTerFkParaUser()
    {
        var model = BuildModel();
        var invitation = model.FindEntityType(typeof(Invitation))!;

        var fk = invitation.GetForeignKeys()
            .Single(f => f.PrincipalEntityType.ClrType == typeof(User));

        fk.Should().NotBeNull();
    }
}
