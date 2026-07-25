using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-033 — Situação apresentada da Corretora (derivada no servidor).</summary>
[Trait("RuleId", "RN-033")]
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
}
