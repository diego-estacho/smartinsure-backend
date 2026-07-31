using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartInsure.Infra.Data.Repositories;

namespace SmartInsure.Tests.Infra.Data.Repositories;

/// <summary>
/// RN-064 — as consultas de Vínculo do contexto do Usuário precisam ser traduzíveis para SQL.
/// <c>ToQueryString</c> compila a consulta no provider real (SQL Server) sem abrir conexão, então
/// pega erro de tradução (ex.: <c>OrderBy</c> sobre membro de DTO já projetado) que os testes de
/// caso de uso, com repositório mockado, nunca alcançam.
/// </summary>
public class UserRepositoryQueryTranslationTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static global::SmartInsure.Infra.Data.Context.SmartInsureDbContext CreateContext()
        => new(new DbContextOptionsBuilder<global::SmartInsure.Infra.Data.Context.SmartInsureDbContext>()
            .UseSqlServer("Server=none;Database=none;Trusted_Connection=False;")
            .Options);

    [Fact]
    [Trait("RuleId", "RN-064")]
    public void BrokerageMembershipsQuery_DeveTraduzirParaSqlOrdenadaPeloNomeDaCorretora()
    {
        using var context = CreateContext();

        var sql = UserRepository.BrokerageMembershipsQuery(context, UserId).ToQueryString();

        sql.Should().Contain("UserBrokerageMemberships");
        sql.Should().Contain("Persons");
        sql.Should().Contain("Profiles");
        sql.Should().Contain("ORDER BY");
    }

    [Fact]
    [Trait("RuleId", "RN-064")]
    public void PolicyHolderMembershipsQuery_DeveTraduzirParaSqlOrdenadaPeloNomeDoTomador()
    {
        using var context = CreateContext();

        var sql = UserRepository.PolicyHolderMembershipsQuery(context, UserId).ToQueryString();

        sql.Should().Contain("UserPolicyHolderMemberships");
        sql.Should().Contain("Persons");
        sql.Should().Contain("Profiles");
        sql.Should().Contain("ORDER BY");
    }
}
