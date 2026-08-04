using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.QuotationGroupUseCases.CreateQuotationGroup;

/// <summary>
/// RN-503 — o endereço do Segurado escolhido pelo corretor é replicado para a oferta na criação. A
/// escolha vem no contrato; o Grupo guarda a cópia, não uma referência ao cadastro da Pessoa.
/// </summary>
[Trait("RuleId", "RN-503")]
public class CreateQuotationGroupInsuredAddressTests
{
    private readonly IQuotationGroupRepository _quotationGroupRepository =
        Substitute.For<IQuotationGroupRepository>();

    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly IModalityRepository _modalityRepository = Substitute.For<IModalityRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateQuotationGroupUseCase _useCase;

    private readonly Guid _policyHolderId = Guid.CreateVersion7();
    private readonly Guid _insuredId = Guid.CreateVersion7();
    private readonly Guid _modalityId = Guid.CreateVersion7();

    private Person _insured = null!;

    public CreateQuotationGroupInsuredAddressTests()
        => _useCase = new CreateQuotationGroupUseCase(
            _quotationGroupRepository, _personRepository, _modalityRepository, _unitOfWork);

    private void SetupReferences()
    {
        var policyHolder = Person.Create("11444777000161", "Tomador Ltda", null, Guid.NewGuid());
        policyHolder.AssignRole(EPersonRole.PolicyHolder);

        _insured = Person.Create("11444777000242", "Segurado Ltda", null, Guid.NewGuid());
        _insured.AssignRole(EPersonRole.Insured);
        _insured.AddMainAddress("04538133", "Avenida Faria Lima", "3477", null, "Itaim", "São Paulo", "SP");
        _insured.AddAdditionalAddress("01310930", "Avenida Paulista", "1578", "10º andar", "Bela Vista", "São Paulo", "SP");

        _personRepository.GetByIdWithRolesAsync(_policyHolderId, Arg.Any<CancellationToken>()).Returns(policyHolder);
        _personRepository.GetByIdWithRolesAsync(_insuredId, Arg.Any<CancellationToken>()).Returns(_insured);
        _modalityRepository.GetByIdAsync(_modalityId, Arg.Any<CancellationToken>())
            .Returns(Modality.CreateManual("Garantia de Execução", null, EModalityStatus.Active));
    }

    private CreateQuotationGroupRequest Request(Guid? insuredAddressId)
        => new(
            _policyHolderId, null, _insuredId, _modalityId,
            1_000m, new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            "All", [], false, false, insuredAddressId);

    [Fact]
    public async Task Execute_DeveReplicarOEnderecoEscolhidoDoSegurado()
    {
        SetupReferences();
        var chosen = _insured.Addresses.Single(address => !address.IsMain);
        QuotationGroup? persisted = null;
        await _quotationGroupRepository.AddAsync(
            Arg.Do<QuotationGroup>(group => persisted = group), Arg.Any<CancellationToken>());

        await _useCase.ExecuteAsync(Request(chosen.Id), CancellationToken.None);

        persisted.Should().NotBeNull();
        persisted!.InsuredAddress.Should().NotBeNull();
        persisted.InsuredAddress!.Street.Should().Be("Avenida Paulista");
        persisted.InsuredAddress.ZipCode.Should().Be("01310930");
        persisted.InsuredAddress.Complement.Should().Be("10º andar");
    }

    [Fact]
    public async Task Execute_SemEnderecoEscolhido_DeveReplicarOEnderecoPrincipal()
    {
        SetupReferences();
        QuotationGroup? persisted = null;
        await _quotationGroupRepository.AddAsync(
            Arg.Do<QuotationGroup>(group => persisted = group), Arg.Any<CancellationToken>());

        await _useCase.ExecuteAsync(Request(insuredAddressId: null), CancellationToken.None);

        persisted!.InsuredAddress!.Street.Should().Be("Avenida Faria Lima");
    }

    [Fact]
    public async Task Execute_EnderecoQueNaoPertenceAoSegurado_DeveRecusar()
    {
        SetupReferences();

        var act = () => _useCase.ExecuteAsync(Request(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*endereço*");
        await _quotationGroupRepository.DidNotReceive()
            .AddAsync(Arg.Any<QuotationGroup>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// RN-503 (caso limite): Segurado ainda sem endereço não impede montar a oferta — quem cobra é o
    /// portão do emitir (RN-500), que explica o que falta e manda corrigir no cadastro do Segurado.
    /// </summary>
    [Fact]
    public async Task Execute_SeguradoSemEndereco_DeveCriarAOfertaSemReplica()
    {
        var policyHolder = Person.Create("11444777000161", "Tomador Ltda", null, Guid.NewGuid());
        policyHolder.AssignRole(EPersonRole.PolicyHolder);

        var insuredWithoutAddress = Person.Create("11444777000242", "Segurado Ltda", null, Guid.NewGuid());
        insuredWithoutAddress.AssignRole(EPersonRole.Insured);

        _personRepository.GetByIdWithRolesAsync(_policyHolderId, Arg.Any<CancellationToken>()).Returns(policyHolder);
        _personRepository.GetByIdWithRolesAsync(_insuredId, Arg.Any<CancellationToken>())
            .Returns(insuredWithoutAddress);
        _modalityRepository.GetByIdAsync(_modalityId, Arg.Any<CancellationToken>())
            .Returns(Modality.CreateManual("Garantia de Execução", null, EModalityStatus.Active));

        QuotationGroup? persisted = null;
        await _quotationGroupRepository.AddAsync(
            Arg.Do<QuotationGroup>(group => persisted = group), Arg.Any<CancellationToken>());

        await _useCase.ExecuteAsync(Request(insuredAddressId: null), CancellationToken.None);

        persisted.Should().NotBeNull();
        persisted!.InsuredAddress.Should().BeNull();
        persisted.HasInsuredAddressForIssuance().Should().BeFalse("o emitir é que cobra o endereço (RN-500)");
    }
}
