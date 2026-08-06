using FluentAssertions;
using SmartInsure.Core.Constants;

namespace SmartInsure.Tests.Core.Constants;

/// <summary>RN-513 — Permissão própria para emitir Apólice, declarada no catálogo fixo.</summary>
public class PermissionCatalogTests
{
    [Fact]
    [Trait("RuleId", "RN-513")]
    public void Catalogo_DeveDeclararPermissaoDeEmitirApolice()
    {
        PermissionCodes.All.Should().Contain(PermissionCodes.PoliciesIssue);
    }

    [Fact]
    [Trait("RuleId", "RN-513")]
    public void PermissaoDeEmitirApolice_DeveSerDistintaDasDeGrupoDeCotacao()
    {
        PermissionCodes.PoliciesIssue.Should().NotBe(PermissionCodes.QuotationGroupsCreate);
        PermissionCodes.PoliciesIssue.Should().NotBe(PermissionCodes.QuotationGroupsEdit);
    }
}
