using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-014/RN-015/RN-016 — Pessoa física/jurídica com documento canônico e endereço principal único.</summary>
public class PersonTests
{
    [Fact]
    [Trait("RuleId", "RN-014")]
    public void Create_DeveNormalizarDocumentoENomes_QuandoPessoaJuridica()
    {
        var person = Person.Create(
            "11.444.777/0001-61", "  Alfa Ltda  ", "  ", Guid.NewGuid());

        person.DocumentNumber.Should().Be("11444777000161");
        person.Name.Should().Be("Alfa Ltda");
        person.SocialName.Should().BeNull();
        person.Type.Should().Be(EPersonType.J);
    }

    [Fact]
    [Trait("RuleId", "RN-015")]
    public void Create_DeveCriarPessoaFisicaSemNaturezaJuridica_QuandoCpf()
    {
        var person = Person.Create("529.982.247-25", "Maria Silva", null, null);

        person.DocumentNumber.Should().Be("52998224725");
        person.Type.Should().Be(EPersonType.F);
        person.LegalNatureId.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-015")]
    public void Create_DeveRecusarPessoaJuridica_QuandoSemNaturezaJuridica()
    {
        var action = () => Person.Create("11444777000161", "Alfa Ltda", null, null);

        action.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    [Trait("RuleId", "RN-015")]
    public void Create_DeveRecusarPessoaFisica_QuandoComNaturezaJuridica()
    {
        var action = () => Person.Create("52998224725", "Maria Silva", null, Guid.NewGuid());

        action.Should().Throw<BusinessRuleException>();
    }

    [Theory]
    [InlineData("123")]
    [InlineData("")]
    [Trait("RuleId", "RN-014")]
    public void Create_DeveRecusar_QuandoDocumentoInvalido(string documentNumber)
    {
        var action = () => Person.Create(documentNumber, "Alfa Ltda", null, Guid.NewGuid());

        action.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    [Trait("RuleId", "RN-014")]
    public void Create_DeveRecusar_QuandoNomeAusente()
    {
        var action = () => Person.Create("11444777000161", " ", null, Guid.NewGuid());

        action.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    [Trait("RuleId", "RN-014")]
    public void AddMainAddress_DeveRecusarSegundoPrincipal_QuandoJaExiste()
    {
        var person = Person.Create("11444777000161", "Alfa Ltda", null, Guid.NewGuid());
        person.AddMainAddress("01310100", "Avenida Paulista", "1000", null, "Bela Vista", "São Paulo", "sp");

        var action = () => person.AddMainAddress(null, null, null, null, null, null, null);

        action.Should().Throw<ConflictException>();
        person.Addresses.Should().ContainSingle(address => address.IsMain && address.State == "SP");
    }

    [Fact]
    [Trait("RuleId", "RN-017")]
    public void AssignRole_DeveAcumularPapeisSemDuplicar()
    {
        var person = Person.Create("52998224725", "Maria Silva", null, null);

        person.AssignRole(EPersonRole.Insured);
        person.AssignRole(EPersonRole.Broker);
        person.AssignRole(EPersonRole.Insured);

        person.Roles.Should().HaveCount(2);
        person.Roles.Should().ContainSingle(role => role.Role == EPersonRole.Insured);
        person.Roles.Should().ContainSingle(role => role.Role == EPersonRole.Broker);
    }

    [Fact]
    [Trait("RuleId", "RN-019")]
    public void AssignRole_DeveCriarPapelAtivo_QuandoVinculaCorretor()
    {
        var person = Person.Create("11444777000161", "Alfa Ltda", null, Guid.NewGuid());

        person.AssignRole(EPersonRole.Broker);

        person.GetRole(EPersonRole.Broker)!.Status.Should().Be(EPersonRoleStatus.Active);
    }

    [Fact]
    [Trait("RuleId", "RN-021")]
    public void PersonRole_DeveAtivarEInativar_QuandoSituacaoMuda()
    {
        var person = Person.Create("11444777000161", "Alfa Ltda", null, Guid.NewGuid());
        person.AssignRole(EPersonRole.Broker);
        var role = person.GetRole(EPersonRole.Broker)!;

        role.Deactivate();
        role.Status.Should().Be(EPersonRoleStatus.Inactive);

        role.Activate();
        role.Status.Should().Be(EPersonRoleStatus.Active);
    }

    [Fact]
    [Trait("RuleId", "RN-021")]
    public void PersonRole_DeveRecusar_QuandoSituacaoJaEAPedida()
    {
        var person = Person.Create("11444777000161", "Alfa Ltda", null, Guid.NewGuid());
        person.AssignRole(EPersonRole.Broker);
        var role = person.GetRole(EPersonRole.Broker)!;

        var action = role.Activate;

        action.Should().Throw<ConflictException>();
    }

    [Theory]
    [InlineData("11444777000161", true)]
    [InlineData("11444777000242", false)]
    [Trait("RuleId", "RN-016")]
    public void IsHeadquarters_DeveIdentificarMatrizPelaOrdemDoCnpj(string cnpj, bool expected)
    {
        var person = Person.Create(cnpj, "Alfa Ltda", null, Guid.NewGuid());

        person.IsHeadquarters.Should().Be(expected);
    }

    [Fact]
    [Trait("RuleId", "RN-016")]
    public void IsHeadquarters_DeveSerFalso_QuandoPessoaFisica()
    {
        var person = Person.Create("52998224725", "Maria Silva", null, null);

        person.IsHeadquarters.Should().BeFalse();
    }

    [Fact]
    [Trait("RuleId", "RN-101")]
    public void LinkToHeadquarters_DeveVincularFilialAMatrizDaMesmaRaiz()
    {
        var headquarters = Person.Create("11222333000181", "Matriz LTDA", null, Guid.NewGuid());
        var branch = Person.Create("11222333000262", "Filial LTDA", null, Guid.NewGuid());

        branch.LinkToHeadquarters(headquarters);

        branch.HeadquartersPersonId.Should().Be(headquarters.Id);
        headquarters.HeadquartersPersonId.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-101")]
    public void LinkToHeadquarters_DeveRecusarMatrizDeOutraRaiz()
    {
        var headquarters = Person.Create("11222333000181", "Matriz LTDA", null, Guid.NewGuid());
        var branch = Person.Create("99888777000282", "Outra Raiz", null, Guid.NewGuid());

        var act = () => branch.LinkToHeadquarters(headquarters);

        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    [Trait("RuleId", "RN-101")]
    public void LinkToHeadquarters_DeveRecusarMatrizComoFilialDeSiMesma()
    {
        var headquarters = Person.Create("11222333000181", "Matriz LTDA", null, Guid.NewGuid());
        var other = Person.Create("11222333000181", "Matriz LTDA", null, Guid.NewGuid());

        var act = () => headquarters.LinkToHeadquarters(other);

        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    [Trait("RuleId", "RN-101")]
    public void LinkToHeadquarters_DeveSerIdempotente()
    {
        var headquarters = Person.Create("11222333000181", "Matriz LTDA", null, Guid.NewGuid());
        var branch = Person.Create("11222333000262", "Filial LTDA", null, Guid.NewGuid());

        branch.LinkToHeadquarters(headquarters);
        branch.LinkToHeadquarters(headquarters);

        branch.HeadquartersPersonId.Should().Be(headquarters.Id);
    }

    [Fact]
    [Trait("RuleId", "RN-101")]
    public void LinkToHeadquarters_DeveRecusarPessoaFisica_QuandoTentaVincularAMatriz()
    {
        var headquarters = Person.Create("11222333000181", "Matriz LTDA", null, Guid.NewGuid());

        // CPF (11 dígitos, dígitos verificadores válidos) com a mesma raiz da matriz —
        // isola o guard de Type != J: sem ele, o guard de raiz também não pegaria.
        var person = Person.Create("112.223.339-61", "Pessoa Fisica", null, null);

        var act = () => person.LinkToHeadquarters(headquarters);

        act.Should().Throw<BusinessRuleException>();
        person.Type.Should().Be(EPersonType.F);
    }

    [Fact]
    [Trait("RuleId", "RN-101")]
    public void LinkToHeadquarters_DeveRecusarArgumentoQueNaoEMatriz()
    {
        var branch = Person.Create("11222333000262", "Filial LTDA", null, Guid.NewGuid());

        // Estabelecimento da mesma raiz, ordem /0003 — não é matriz (/0001).
        var notHeadquarters = Person.Create("11222333000343", "Outra Filial LTDA", null, Guid.NewGuid());

        var act = () => branch.LinkToHeadquarters(notHeadquarters);

        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    [Trait("RuleId", "RN-101")]
    public void LinkToHeadquarters_DeveRecusarRevinculoAMatrizDiferente()
    {
        var firstHeadquarters = Person.Create("11222333000181", "Matriz A LTDA", null, Guid.NewGuid());
        var secondHeadquarters = Person.Create("11222333000181", "Matriz B LTDA", null, Guid.NewGuid());
        var branch = Person.Create("11222333000262", "Filial LTDA", null, Guid.NewGuid());

        branch.LinkToHeadquarters(firstHeadquarters);

        var act = () => branch.LinkToHeadquarters(secondHeadquarters);

        act.Should().Throw<ConflictException>();
    }
}
