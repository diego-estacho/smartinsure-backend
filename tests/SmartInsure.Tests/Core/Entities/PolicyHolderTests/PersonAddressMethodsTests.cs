using FluentAssertions;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Core.Entities.PolicyHolderTests;

[Trait("Category", "Domain")]
public sealed class PersonAddressMethodsTests
{
    [Fact]
    [Trait("RuleId", "RN-026")]
    public void AddAdditionalAddress_DeveAdicionarEndereçoComIsMainFalse()
    {
        var person = Person.Create("12345678901234", "Empresa Test", null, Guid.NewGuid());
        person.AddMainAddress(null, null, null, null, null, null, null);

        person.AddAdditionalAddress("12345678", "Rua Teste", "123", null, "Bairro", "Cidade", "SP");

        person.Addresses.Should().HaveCount(2);
        var additional = person.Addresses.FirstOrDefault(a => !a.IsMain);
        additional.Should().NotBeNull();
        additional!.Street.Should().Be("Rua Teste");
    }

    [Fact]
    [Trait("RuleId", "RN-026")]
    public void UpdateAdditionalAddress_DeveAtualizarEndereçoComplementar()
    {
        var person = Person.Create("12345678901234", "Empresa Test", null, Guid.NewGuid());
        person.AddMainAddress(null, null, null, null, null, null, null);
        person.AddAdditionalAddress("12345678", "Rua Antiga", "123", null, "Bairro", "Cidade", "SP");
        var addressId = person.Addresses.First(a => !a.IsMain).Id;

        person.UpdateAdditionalAddress(addressId, "87654321", "Rua Nova", "456", null, "Novo Bairro", "Nova Cidade", "RJ");

        var updated = person.Addresses.First(a => a.Id == addressId);
        updated.Street.Should().Be("Rua Nova");
        updated.ZipCode.Should().Be("87654321");
    }

    [Fact]
    [Trait("RuleId", "RN-026")]
    public void UpdateAdditionalAddress_DeveLançarBusinessRuleException_QuandoPrincipal()
    {
        var person = Person.Create("12345678901234", "Empresa Test", null, Guid.NewGuid());
        person.AddMainAddress(null, null, null, null, null, null, null);
        var mainAddressId = person.Addresses.First(a => a.IsMain).Id;

        var action = () => person.UpdateAdditionalAddress(mainAddressId, null, "Nova Rua", null, null, null, null, null);

        action.Should().ThrowExactly<BusinessRuleException>()
            .WithMessage("O endereço principal não pode ser alterado ou removido.");
    }

    [Fact]
    [Trait("RuleId", "RN-026")]
    public void UpdateAdditionalAddress_DeveLançarNotFoundException_QuandoIdInexistente()
    {
        var person = Person.Create("12345678901234", "Empresa Test", null, Guid.NewGuid());
        person.AddMainAddress(null, null, null, null, null, null, null);

        var action = () => person.UpdateAdditionalAddress(Guid.NewGuid(), null, "Nova Rua", null, null, null, null, null);

        action.Should().ThrowExactly<NotFoundException>()
            .WithMessage("Endereço não encontrado.");
    }

    [Fact]
    [Trait("RuleId", "RN-026")]
    public void RemoveAdditionalAddress_DeveRemoverEndereçoComplementar()
    {
        var person = Person.Create("12345678901234", "Empresa Test", null, Guid.NewGuid());
        person.AddMainAddress(null, null, null, null, null, null, null);
        person.AddAdditionalAddress("12345678", "Rua Teste", "123", null, "Bairro", "Cidade", "SP");
        var addressIdToRemove = person.Addresses.First(a => !a.IsMain).Id;

        person.RemoveAdditionalAddress(addressIdToRemove);

        person.Addresses.Should().HaveCount(1);
        person.Addresses.Should().AllSatisfy(a => a.IsMain.Should().BeTrue());
    }

    [Fact]
    [Trait("RuleId", "RN-026")]
    public void RemoveAdditionalAddress_DeveLançarBusinessRuleException_QuandoPrincipal()
    {
        var person = Person.Create("12345678901234", "Empresa Test", null, Guid.NewGuid());
        person.AddMainAddress(null, null, null, null, null, null, null);
        var mainAddressId = person.Addresses.First(a => a.IsMain).Id;

        var action = () => person.RemoveAdditionalAddress(mainAddressId);

        action.Should().ThrowExactly<BusinessRuleException>()
            .WithMessage("O endereço principal não pode ser alterado ou removido.");
    }

    [Fact]
    [Trait("RuleId", "RN-026")]
    public void RemoveAdditionalAddress_DeveLançarNotFoundException_QuandoIdInexistente()
    {
        var person = Person.Create("12345678901234", "Empresa Test", null, Guid.NewGuid());
        person.AddMainAddress(null, null, null, null, null, null, null);

        var action = () => person.RemoveAdditionalAddress(Guid.NewGuid());

        action.Should().ThrowExactly<NotFoundException>()
            .WithMessage("Endereço não encontrado.");
    }

    [Fact]
    [Trait("RuleId", "RN-026")]
    public void DeveManterSempreUmEndereçoPrincipal_QuandoOperacoesAdicionais()
    {
        var person = Person.Create("12345678901234", "Empresa Test", null, Guid.NewGuid());
        person.AddMainAddress(null, null, null, null, null, null, null);
        person.AddAdditionalAddress("12345678", "Rua 1", "1", null, "B1", "C1", "SP");
        person.AddAdditionalAddress("87654321", "Rua 2", "2", null, "B2", "C2", "RJ");

        person.Addresses.Where(a => a.IsMain).Should().HaveCount(1);
        person.Addresses.Where(a => !a.IsMain).Should().HaveCount(2);
    }
}
