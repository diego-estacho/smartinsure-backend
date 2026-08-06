using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance.Interfaces;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance.Requests;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance.Responses;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RequestPolicyIssuance;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RequestPolicyIssuance.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.IssuanceUseCases;

/// <summary>
/// RN-502/RN-506/RN-509/RN-511/RN-514 — sequência do emitir: reenvia os termos vigentes, comunica o
/// aceite, solicita a emissão e registra a Apólice, promovendo a oferta a Emissão solicitada. Recusa da
/// Seguradora não registra nada. Emitida uma Cotação, as irmãs são canceladas — e falhar nisso não
/// desfaz a emissão.
/// </summary>
public class RequestPolicyIssuanceSuccessTests
{
    private readonly IQuotationGroupRepository _groupRepository = Substitute.For<IQuotationGroupRepository>();
    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly IPolicyRepository _policyRepository = Substitute.For<IPolicyRepository>();
    private readonly IBrokerageInsurerEnablementRepository _enablementRepository =
        Substitute.For<IBrokerageInsurerEnablementRepository>();
    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly IInsurerRepository _insurerRepository = Substitute.For<IInsurerRepository>();
    private readonly IRegisterTermAcceptanceUseCase _registerTermAcceptance =
        Substitute.For<IRegisterTermAcceptanceUseCase>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IImportedModalityRepository _importedModalityRepository =
        Substitute.For<IImportedModalityRepository>();
    private readonly IImportedModalityTagRepository _tagRepository =
        Substitute.For<IImportedModalityTagRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICalculationEngine _engine = Substitute.For<ICalculationEngine>();

    private readonly Guid _brokerageId = Guid.CreateVersion7();
    private readonly Guid _insurerId = Guid.CreateVersion7();
    private readonly Guid _siblingInsurerId = Guid.CreateVersion7();
    private readonly Guid _termAcceptanceId = Guid.CreateVersion7();
    private const string ExternalIdentity = "casdoor|diegoteste01";

    private QuotationGroup _group = null!;
    private Quotation _quotation = null!;
    private Quotation _sibling = null!;
    private User _user = null!;

    private RequestPolicyIssuanceUseCase BuildUseCase(bool withSibling = false)
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<ICalculationEngine>(ECalculationEngine.PlugV2, (_, _) => _engine);

        _group = QuotationGroup.Create(
            Guid.CreateVersion7(), null, Guid.CreateVersion7(), Guid.CreateVersion7(), 100_000m,
            new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            EQuotationScopeMode.All, [], []);
        _group.AssignBrokerage(_brokerageId);
        _group.ReplicateInsuredAddress(
            "01310930", "Avenida Paulista", "1578", "10º andar", "Bela Vista", "São Paulo", "SP");

        var enablement = BrokerageInsurerEnablement.Create(
            _brokerageId, _insurerId, ECalculationEngine.PlugV2, "{\"baseUrl\":\"https://x/\",\"key\":\"k\"}");

        _quotation = BuildQuotation(_insurerId, enablement.Id, "prop-escolhida", 300m);
        _group.MarkQuoted();
        _group.SelectQuotation(_quotation.Id);

        var siblings = new List<Quotation> { _quotation };

        if (withSibling)
        {
            var siblingEnablement = BrokerageInsurerEnablement.Create(
                _brokerageId, _siblingInsurerId, ECalculationEngine.PlugV2, "{\"baseUrl\":\"https://y/\",\"key\":\"k2\"}");
            _sibling = BuildQuotation(_siblingInsurerId, siblingEnablement.Id, "prop-irma", 320m);
            siblings.Add(_sibling);

            _enablementRepository.GetByIdAsync(siblingEnablement.Id, Arg.Any<CancellationToken>())
                .Returns(siblingEnablement);
        }

        _user = User.Create("Diego", "diego@onpoint.com.br", ExternalIdentity);

        // A emissão carrega o Grupo COM a réplica do endereço (RN-503).
        _groupRepository.GetByIdWithInsuredAddressAsync(_group.Id, Arg.Any<CancellationToken>()).Returns(_group);
        _quotationRepository.GetByIdAsync(_quotation.Id, Arg.Any<CancellationToken>()).Returns(_quotation);
        _quotationRepository.ListByGroupAsync(_group.Id, Arg.Any<CancellationToken>()).Returns(siblings);
        _enablementRepository.GetByIdAsync(enablement.Id, Arg.Any<CancellationToken>()).Returns(enablement);
        _policyRepository.ExistsForQuotationAsync(_quotation.Id, Arg.Any<CancellationToken>()).Returns(false);
        _personRepository.GetByIdAsync(_brokerageId, Arg.Any<CancellationToken>())
            .Returns(Person.Create("12345678000195", "Corretora", null, Guid.CreateVersion7()));
        _insurerRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Insurer.Create("11222333000181", "Seguradora X", null, null, EInsurerStatus.Active, "REF-1"));
        _currentUser.UserIdentifier.Returns(ExternalIdentity);
        _userRepository.GetByExternalIdentityAsync(ExternalIdentity, Arg.Any<CancellationToken>()).Returns(_user);
        _registerTermAcceptance.ExecuteAsync(Arg.Any<RegisterTermAcceptanceRequest>(), Arg.Any<CancellationToken>())
            .Returns(new RegisterTermAcceptanceResponse
            {
                TermAcceptanceId = _termAcceptanceId,
                AcceptedAt = DateTime.UtcNow,
            });

        return new RequestPolicyIssuanceUseCase(
            _groupRepository, _quotationRepository, _policyRepository, _enablementRepository,
            _personRepository, _insurerRepository, _registerTermAcceptance, _userRepository,
            // RN-502: o portão consulta o catálogo para saber se a Modalidade define Tag. Sem catálogo
            // importado (padrão dos substitutos), não há minuta a exigir — o caminho de sucesso segue.
            _importedModalityRepository, _tagRepository,
            _currentUser, _unitOfWork, services.BuildServiceProvider());
    }

    private Quotation BuildQuotation(Guid insurerId, Guid enablementId, string proposalExternalId, decimal premium)
    {
        var quotation = Quotation.Requested(_group.Id, insurerId);
        quotation.MarkObtained(
            EQuotationResult.ReadyForEmission, null, premium, 20m, 60m, 1.5m, 500_000m,
            proposalExternalId, "PROP-1", false, null, false, [], DateTime.UtcNow);
        quotation.SetEnablement(enablementId);
        quotation.SetProviderOptions([new QuotationInstallmentOption { Number = 1, Value = premium }], [0, 30], []);
        quotation.SetMinuta(
            JsonSerializer.Serialize(new[] { new { name = "objeto", value = "Contrato 2026/0481" } }), "[]");

        return quotation;
    }

    private RequestPolicyIssuanceRequest Request()
        => new()
        {
            QuotationGroupId = _group.Id,
            InstallmentNumber = 1,
            GracePeriodInDays = 30,
            TermAccepted = true,
            UserAgent = "Mozilla/5.0",
        };

    private void EngineAcceptsIssuance(string policyExternalId = "AP-EXT-1", string? proposalNumber = "PROP-1")
        => _engine.CreatePolicyAsync(Arg.Any<string?>(), Arg.Any<CreatePolicyInput>(), Arg.Any<CancellationToken>())
            .Returns(new PolicyIssuanceResult
            {
                PolicyExternalId = policyExternalId,
                ProposalNumber = proposalNumber,
            });

    [Fact]
    [Trait("RuleId", "RN-514")]
    public async Task Execute_EmissaoAceita_DeveRegistrarApoliceComOsValoresEmitidos()
    {
        var useCase = BuildUseCase();
        EngineAcceptsIssuance();
        Policy? persisted = null;
        await _policyRepository.AddAsync(
            Arg.Do<Policy>(policy => persisted = policy), Arg.Any<CancellationToken>());

        var response = await useCase.ExecuteAsync(Request(), CancellationToken.None);

        persisted.Should().NotBeNull();
        persisted!.QuotationId.Should().Be(_quotation.Id);
        persisted.QuotationGroupId.Should().Be(_group.Id);
        persisted.PolicyExternalId.Should().Be("AP-EXT-1");
        persisted.ProposalNumber.Should().Be("PROP-1");
        persisted.Premium.Should().Be(300m);
        persisted.Tax.Should().Be(1.5m);
        persisted.CommissionPercentage.Should().Be(20m);
        persisted.CommissionValue.Should().Be(60m);
        persisted.InstallmentNumber.Should().Be(1);
        persisted.GracePeriodInDays.Should().Be(30);
        persisted.TermAcceptanceId.Should().Be(_termAcceptanceId);
        persisted.RequestedByUserId.Should().Be(_user.Id);
        persisted.InsuredAddressStreet.Should().Be("Avenida Paulista");
        persisted.InsuredAddressComplement.Should().Be("10º andar");
        response.PolicyId.Should().Be(persisted.Id);
        response.QuotationGroupStatus.Should().Be(EQuotationGroupStatus.EmissionRequested.ToString());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-508")]
    public async Task Execute_EmissaoAceita_DevePromoverAOfertaParaEmissaoSolicitada()
    {
        var useCase = BuildUseCase();
        EngineAcceptsIssuance();

        await useCase.ExecuteAsync(Request(), CancellationToken.None);

        _group.Status.Should().Be(EQuotationGroupStatus.EmissionRequested);
        _groupRepository.Received().Update(_group);
    }

    [Fact]
    [Trait("RuleId", "RN-502")]
    public async Task Execute_DeveReenviarOsTermosVigentesEComunicarOAceiteAntesDeEmitir()
    {
        var useCase = BuildUseCase();
        EngineAcceptsIssuance();

        await useCase.ExecuteAsync(Request(), CancellationToken.None);

        Received.InOrder(async () =>
        {
            await _engine.SubmitProposalTermsAsync(
                Arg.Any<string?>(), Arg.Any<SubmitProposalTermsInput>(), Arg.Any<CancellationToken>());
            await _engine.SubmitPolicyAcceptanceTermAsync(
                Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
            await _engine.CreatePolicyAsync(
                Arg.Any<string?>(), Arg.Any<CreatePolicyInput>(), Arg.Any<CancellationToken>());
        });
    }

    /// <summary>
    /// RN-502 (caso limite): sem Tag nem Cláusula na minuta não há termo a reenviar, e o provedor recusa
    /// um envio vazio ("Nenhum termo foi informado para atualização" — defeito visto no E2E). "Nada a
    /// preencher" tem de significar nada a enviar, seguindo direto para o pedido de emissão.
    /// </summary>
    [Fact]
    [Trait("RuleId", "RN-502")]
    public async Task Execute_SemMinutaAEnviar_NaoDeveChamarOEnvioDeTermos()
    {
        var useCase = BuildUseCase();
        _quotation.SetMinuta(null, null);
        EngineAcceptsIssuance();

        await useCase.ExecuteAsync(Request(), CancellationToken.None);

        await _engine.DidNotReceive().SubmitProposalTermsAsync(
            Arg.Any<string?>(), Arg.Any<SubmitProposalTermsInput>(), Arg.Any<CancellationToken>());
        await _engine.Received(1).CreatePolicyAsync(
            Arg.Any<string?>(), Arg.Any<CreatePolicyInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-502")]
    public async Task Execute_FalhaNoEnvioDosTermos_NaoDeveEmitirNemRegistrarNada()
    {
        var useCase = BuildUseCase();
        _engine.SubmitProposalTermsAsync(
                Arg.Any<string?>(), Arg.Any<SubmitProposalTermsInput>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new CalculationEngineException("Falha ao enviar os termos."));

        var act = () => useCase.ExecuteAsync(Request(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _engine.DidNotReceive().CreatePolicyAsync(
            Arg.Any<string?>(), Arg.Any<CreatePolicyInput>(), Arg.Any<CancellationToken>());
        await _policyRepository.DidNotReceive().AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
        _group.Status.Should().Be(EQuotationGroupStatus.Quoted);
    }

    [Fact]
    [Trait("RuleId", "RN-511")]
    public async Task Execute_RecusaDaSeguradora_NaoDeveRegistrarApoliceEDeveManterAOfertaCotada()
    {
        var useCase = BuildUseCase();
        _engine.CreatePolicyAsync(Arg.Any<string?>(), Arg.Any<CreatePolicyInput>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new CalculationEngineException("Vigência retroativa não permitida."));

        var act = () => useCase.ExecuteAsync(Request(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*retroativa*");
        await _policyRepository.DidNotReceive().AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
        _group.Status.Should().Be(EQuotationGroupStatus.Quoted);
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-503")]
    public async Task Execute_DeveEnviarOEnderecoReplicadoDaOfertaAoProvedor()
    {
        var useCase = BuildUseCase();
        EngineAcceptsIssuance();
        CreatePolicyInput? sent = null;
        await _engine.CreatePolicyAsync(
            Arg.Any<string?>(), Arg.Do<CreatePolicyInput>(input => sent = input), Arg.Any<CancellationToken>());

        await useCase.ExecuteAsync(Request(), CancellationToken.None);

        sent.Should().NotBeNull();
        sent!.InsuredAddress.ZipCode.Should().Be("01310930");
        sent.InsuredAddress.City.Should().Be("São Paulo");
        sent.InsuredAddress.State.Should().Be("SP");
        sent.ProposalExternalId.Should().Be("prop-escolhida");
        sent.InstallmentNumber.Should().Be(1);
        sent.GracePeriodInDays.Should().Be(30);
    }

    [Fact]
    [Trait("RuleId", "RN-509")]
    public async Task Execute_DeveCancelarAsCotacoesIrmasDoGrupo()
    {
        var useCase = BuildUseCase(withSibling: true);
        EngineAcceptsIssuance();
        var cancelled = new List<CancelProposalInput>();
        await _engine.CancelProposalAsync(
            Arg.Any<string?>(), Arg.Do<CancelProposalInput>(cancelled.Add), Arg.Any<CancellationToken>());

        await useCase.ExecuteAsync(Request(), CancellationToken.None);

        cancelled.Should().ContainSingle();
        cancelled[0].ProposalExternalId.Should().Be("prop-irma");
        cancelled[0].Reason.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    [Trait("RuleId", "RN-509")]
    public async Task Execute_FalhaAoCancelarIrma_NaoDeveDesfazerAEmissao()
    {
        var useCase = BuildUseCase(withSibling: true);
        EngineAcceptsIssuance();
        _engine.CancelProposalAsync(
                Arg.Any<string?>(), Arg.Any<CancelProposalInput>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new CalculationEngineException("Proposta não pode ser cancelada."));

        var response = await useCase.ExecuteAsync(Request(), CancellationToken.None);

        response.PolicyId.Should().NotBeEmpty();
        _group.Status.Should().Be(EQuotationGroupStatus.EmissionRequested);
        await _policyRepository.Received(1).AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
    }
}
