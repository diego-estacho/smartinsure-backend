using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>
/// RN-503 — Endereço do Segurado da oferta: o endereço escolhido é REPLICADO para a oferta, ficando
/// imune a alteração posterior do cadastro da Pessoa. Corrigir é no cadastro do Segurado; para
/// refletir, o corretor confirma o endereço de novo e a oferta re-replica.
/// </summary>
[Trait("RuleId", "RN-503")]
public class QuotationGroupInsuredAddressTests
{
    private static QuotationGroup NewGroup()
        => QuotationGroup.Create(
            Guid.CreateVersion7(), null, Guid.CreateVersion7(), Guid.CreateVersion7(), 1_000m,
            new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            EQuotationScopeMode.All, [], includesPenaltyCoverage: false, includesLaborCoverage: false);

    [Fact]
    public void ReplicateInsuredAddress_DeveGuardarOsValoresDoEnderecoEscolhido()
    {
        var group = NewGroup();

        group.ReplicateInsuredAddress(
            zipCode: "01310930",
            street: "Avenida Paulista",
            number: "1578",
            complement: "10º andar",
            neighborhood: "Bela Vista",
            city: "São Paulo",
            state: "SP");

        group.InsuredAddress.Should().NotBeNull();
        group.InsuredAddress!.ZipCode.Should().Be("01310930");
        group.InsuredAddress.Street.Should().Be("Avenida Paulista");
        group.InsuredAddress.Number.Should().Be("1578");
        group.InsuredAddress.Complement.Should().Be("10º andar");
        group.InsuredAddress.Neighborhood.Should().Be("Bela Vista");
        group.InsuredAddress.City.Should().Be("São Paulo");
        group.InsuredAddress.State.Should().Be("SP");
    }

    [Fact]
    public void ReplicateInsuredAddress_ChamadaDeNovo_DeveAtualizarAReplicaNoLugar()
    {
        var group = NewGroup();
        group.ReplicateInsuredAddress("01310930", "Avenida Paulista", "1578", null, "Bela Vista", "São Paulo", "SP");
        var replicaId = group.InsuredAddress!.Id;

        group.ReplicateInsuredAddress("04538133", "Avenida Brigadeiro Faria Lima", "3477", "Torre B", "Itaim Bibi", "São Paulo", "SP");

        group.InsuredAddress!.Id.Should().Be(replicaId, "a réplica é a mesma da oferta — atualiza no lugar");
        group.InsuredAddress.Street.Should().Be("Avenida Brigadeiro Faria Lima");
        group.InsuredAddress.Complement.Should().Be("Torre B");
    }

    [Fact]
    public void Grupo_SemEnderecoReplicado_NaoDeveTerEnderecoDoSegurado()
    {
        NewGroup().InsuredAddress.Should().BeNull();
    }

    [Fact]
    public void ReplicateInsuredAddress_SemDadoMinimo_DeveSerRecusado()
    {
        var group = NewGroup();

        var act = () => group.ReplicateInsuredAddress("  ", "  ", null, null, null, "  ", "  ");

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*endereço*");
    }

    [Fact]
    public void ReplicateInsuredAddress_NaoDeveDependerDoCadastroDaPessoaDepoisDeReplicado()
    {
        var legalNatureId = Guid.CreateVersion7();
        var insured = Person.Create("11444777000242", "Segurado", null, legalNatureId);
        insured.AddMainAddress("04538133", "Avenida Faria Lima", "3477", null, "Itaim", "São Paulo", "SP");
        insured.AddAdditionalAddress("01310930", "Avenida Paulista", "1578", null, "Bela Vista", "São Paulo", "SP");
        var chosen = insured.Addresses.Single(address => !address.IsMain);

        var group = NewGroup();
        group.ReplicateInsuredAddress(
            chosen.ZipCode, chosen.Street, chosen.Number, chosen.Complement,
            chosen.Neighborhood, chosen.City, chosen.State);

        // O cadastro da Pessoa muda depois: a oferta segue com o que foi replicado.
        insured.UpdateAdditionalAddress(
            chosen.Id, "99999000", "Rua Trocada", "1", null, "Centro", "Outra Cidade", "RJ");

        group.InsuredAddress!.Street.Should().Be("Avenida Paulista");
        group.InsuredAddress.ZipCode.Should().Be("01310930");
    }
}
