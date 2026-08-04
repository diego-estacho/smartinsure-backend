using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Application.UseCases.QuotationGroupUseCases.UpdateQuotationGroup;

/// <summary>
/// RN-503 — na atualização, informar o endereço RE-REPLICA (é o caminho de correção depois de ajustar o
/// cadastro do Segurado); NÃO informar preserva a réplica que a oferta já tem. Sem isso, uma atualização
/// que não passou pela etapa do Segurado — reidratar a oferta e salvar de novo — trocaria silenciosamente
/// o endereço combinado pelo principal do cadastro.
/// </summary>
[Trait("RuleId", "RN-503")]
public class UpdateQuotationGroupInsuredAddressTests
{
    private readonly IQuotationGroupRepository _groupRepository = Substitute.For<IQuotationGroupRepository>();
    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly IModalityRepository _modalityRepository = Substitute.For<IModalityRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateQuotationGroupUseCase _useCase;

    private readonly Guid _policyHolderId = Guid.CreateVersion7();
    private readonly Guid _insuredId = Guid.CreateVersion7();
    private readonly Guid _modalityId = Guid.CreateVersion7();

    private QuotationGroup _group = null!;
    private Person _insured = null!;

    public UpdateQuotationGroupInsuredAddressTests()
        => _useCase = new UpdateQuotationGroupUseCase(
            _groupRepository, _quotationRepository, _personRepository, _modalityRepository, _unitOfWork);

    private void SetupGroupWithReplicatedAddress()
    {
        var policyHolder = Person.Create("11444777000161", "Tomador Ltda", null, Guid.NewGuid());
        policyHolder.AssignRole(EPersonRole.PolicyHolder);

        _insured = Person.Create("11444777000242", "Segurado Ltda", null, Guid.NewGuid());
        _insured.AssignRole(EPersonRole.Insured);
        _insured.AddMainAddress("04538133", "Avenida Faria Lima", "3477", null, "Itaim", "São Paulo", "SP");
        _insured.AddAdditionalAddress("01310930", "Avenida Paulista", "1578", null, "Bela Vista", "São Paulo", "SP");

        _group = QuotationGroup.Create(
            _policyHolderId, null, _insuredId, _modalityId, 1_000m,
            new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            EQuotationScopeMode.All, [], includesPenaltyCoverage: false, includesLaborCoverage: false);

        // A oferta já foi criada com o endereço adicional escolhido pelo corretor.
        var chosen = _insured.Addresses.Single(address => !address.IsMain);
        _group.ReplicateInsuredAddress(
            chosen.ZipCode, chosen.Street, chosen.Number, chosen.Complement,
            chosen.Neighborhood, chosen.City, chosen.State);

        _groupRepository.GetByIdWithInsurersAsync(_group.Id, Arg.Any<CancellationToken>()).Returns(_group);
        _quotationRepository.ExistsForGroupAsync(_group.Id, Arg.Any<CancellationToken>()).Returns(false);
        _personRepository.GetByIdWithRolesAsync(_policyHolderId, Arg.Any<CancellationToken>()).Returns(policyHolder);
        _personRepository.GetByIdWithRolesAsync(_insuredId, Arg.Any<CancellationToken>()).Returns(_insured);
        _modalityRepository.GetByIdAsync(_modalityId, Arg.Any<CancellationToken>())
            .Returns(Modality.CreateManual("Garantia de Execução", null, EModalityStatus.Active));
    }

    private UpdateQuotationGroupRequest Request(Guid? insuredAddressId)
        => new(
            _group.Id, _policyHolderId, null, _insuredId, _modalityId,
            2_000m, new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            "All", [], false, false, insuredAddressId);

    [Fact]
    public async Task Execute_SemInformarEndereco_DevePreservarAReplicaDaOferta()
    {
        SetupGroupWithReplicatedAddress();

        await _useCase.ExecuteAsync(Request(insuredAddressId: null), CancellationToken.None);

        _group.InsuredAddress!.Street.Should().Be(
            "Avenida Paulista", "atualizar sem passar pela etapa do Segurado não troca o endereço combinado");
    }

    [Fact]
    public async Task Execute_InformandoOutroEndereco_DeveReReplicar()
    {
        SetupGroupWithReplicatedAddress();
        var main = _insured.Addresses.Single(address => address.IsMain);

        await _useCase.ExecuteAsync(Request(main.Id), CancellationToken.None);

        _group.InsuredAddress!.Street.Should().Be("Avenida Faria Lima");
    }

    [Fact]
    public async Task Execute_OfertaSemReplicaEsemEnderecoInformado_DeveReplicarOPrincipal()
    {
        SetupGroupWithReplicatedAddress();
        _group = QuotationGroup.Create(
            _policyHolderId, null, _insuredId, _modalityId, 1_000m,
            new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            EQuotationScopeMode.All, [], includesPenaltyCoverage: false, includesLaborCoverage: false);
        _groupRepository.GetByIdWithInsurersAsync(_group.Id, Arg.Any<CancellationToken>()).Returns(_group);

        await _useCase.ExecuteAsync(Request(insuredAddressId: null), CancellationToken.None);

        _group.InsuredAddress!.Street.Should().Be("Avenida Faria Lima");
    }
}
