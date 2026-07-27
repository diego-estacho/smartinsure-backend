using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-053 — Situação apresentada da Corretora (derivada no servidor).</summary>
[Trait("RuleId", "RN-053")]
public class BrokerageSituationRulesTests
{
    [Fact]
    public void Resolve_DeveSerInativa_QuandoPapelInativo_IndependenteDaCompletude()
    {
        BrokerageSituationRules.Resolve(EPersonRoleStatus.Inactive, "Alfa", "contato@alfa.com.br")
            .Should().Be(EBrokerageSituation.Inactive);
        BrokerageSituationRules.Resolve(EPersonRoleStatus.Inactive, null, null)
            .Should().Be(EBrokerageSituation.Inactive);
    }

    [Fact]
    public void Resolve_DeveSerAtiva_QuandoAtivoECadastroCompleto()
    {
        BrokerageSituationRules.Resolve(EPersonRoleStatus.Active, "Alfa", "contato@alfa.com.br")
            .Should().Be(EBrokerageSituation.Active);
    }

    [Theory]
    [InlineData(null, "contato@alfa.com.br")]
    [InlineData("Alfa", null)]
    [InlineData("", "contato@alfa.com.br")]
    [InlineData("Alfa", " ")]
    public void Resolve_DeveSerIncompleta_QuandoAtivoEFaltaFantasiaOuEmail(
        string? socialName, string? contactEmail)
    {
        BrokerageSituationRules.Resolve(EPersonRoleStatus.Active, socialName, contactEmail)
            .Should().Be(EBrokerageSituation.Incomplete);
    }

    /// <summary>
    /// Paridade da regra única: o predicado <see cref="BrokerageSituationRules.Matches"/> (usado no
    /// filtro/contagem em SQL) deve concordar com <see cref="BrokerageSituationRules.Resolve"/>
    /// (usado na projeção da linha) para toda situação — assim a contagem nunca destoa da situação
    /// apresentada (RN-018/RN-053). Compila a expressão e roda contra a Pessoa em memória.
    /// </summary>
    [Theory]
    [InlineData(true, "Alfa", "contato@alfa.com.br")]
    [InlineData(true, "Alfa", null)]
    [InlineData(true, null, "contato@alfa.com.br")]
    [InlineData(true, null, null)]
    [InlineData(false, "Alfa", "contato@alfa.com.br")]
    [InlineData(false, null, null)]
    public void Matches_DeveConcordarComResolve_ParaCadaSituacao(
        bool active, string? socialName, string? contactEmail)
    {
        var person = BuildBroker(active, socialName, contactEmail);
        var role = person.GetRole(EPersonRole.Broker)!;
        var expected = BrokerageSituationRules.Resolve(role.Status, person.SocialName, role.ContactEmail);

        foreach (var situation in Enum.GetValues<EBrokerageSituation>())
        {
            var matches = BrokerageSituationRules.Matches(situation).Compile()(person);
            matches.Should().Be(
                situation == expected,
                $"a regra em memória (Resolve → {expected}) e a expressão devem concordar em {situation}");
        }
    }

    private static Person BuildBroker(bool active, string? socialName, string? contactEmail)
    {
        var person = Person.Create("11222333000181", "Alfa Corretora Ltda", null, Guid.NewGuid());
        person.SetUpBrokerage(active, socialName, contactEmail, null, null);
        return person;
    }
}
