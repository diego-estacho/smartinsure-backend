using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
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
/// RN-500/RN-501/RN-502/RN-505/RN-506/RN-507 — portão do emitir: a plataforma reprova o que já sabe
/// reprovar ANTES de acionar a Seguradora, com motivo específico. Chamada mutante não é gasta em caminho
/// previsível de recusa, e o aceite do Termo não é queimado por uma emissão que já nasceria bloqueada.
/// </summary>
public class RequestPolicyIssuanceGateTests
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
    private readonly IImportedModalityRepository _importedModalityRepository =
        Substitute.For<IImportedModalityRepository>();
    private readonly IImportedModalityTagRepository _tagRepository =
        Substitute.For<IImportedModalityTagRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICalculationEngine _engine = Substitute.For<ICalculationEngine>();

    private readonly Guid _brokerageId = Guid.CreateVersion7();
    private readonly Guid _insurerId = Guid.CreateVersion7();
    private const string ExternalIdentity = "casdoor|diegoteste01";

    private QuotationGroup _group = null!;
    private Quotation _quotation = null!;
    private BrokerageInsurerEnablement _enablement = null!;

    private RequestPolicyIssuanceUseCase BuildUseCase(
        bool requiresCcg = false,
        bool ccgSigned = false,
        bool withMinuta = true,
        EQuotationResult result = EQuotationResult.ReadyForEmission,
        bool withInsuredAddress = true)
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<ICalculationEngine>(ECalculationEngine.PlugV2, (_, _) => _engine);

        _group = QuotationGroup.Create(
            Guid.CreateVersion7(), null, Guid.CreateVersion7(), Guid.CreateVersion7(), 100_000m,
            new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            EQuotationScopeMode.All, [], []);
        _group.AssignBrokerage(_brokerageId);

        if (withInsuredAddress)
        {
            _group.ReplicateInsuredAddress(
                "01310930", "Avenida Paulista", "1578", null, "Bela Vista", "São Paulo", "SP");
        }

        _enablement = BrokerageInsurerEnablement.Create(
            _brokerageId, _insurerId, ECalculationEngine.PlugV2, "{\"baseUrl\":\"https://x/\",\"key\":\"k\"}");

        _quotation = Quotation.Requested(_group.Id, _insurerId);
        _quotation.MarkObtained(
            result,
            result == EQuotationResult.Analysis ? EAnalysisTrack.Underwriting : null,
            result == EQuotationResult.ReadyForEmission ? 300m : null,
            20m, 60m, 1.5m, 500_000m,
            "11111111-1111-1111-1111-111111111111", "PROP-1",
            requiresCcg, null, ccgSigned,
            result == EQuotationResult.Unavailable ? ["Modalidade indisponível."] : [],
            DateTime.UtcNow);
        _quotation.SetEnablement(_enablement.Id);
        _quotation.SetProviderOptions([new QuotationInstallmentOption { Number = 1, Value = 300m }], [0, 30], []);

        var imported = ImportedModality.Create(
            _insurerId, "src-1", "Licitante", ESuretyBranch.Public, null, null, null, null, DateTime.UtcNow);
        _importedModalityRepository
            .GetActiveByInsurerAndModalityAsync(_insurerId, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(imported);
        _tagRepository.GetByImportedModalityAsync(imported.Id, Arg.Any<CancellationToken>())
            .Returns(ImportedModalityTag.Create(imported.Id, "[{\"name\":\"objeto\"}]", "Objeto"));

        if (withMinuta)
        {
            _quotation.SetMinuta(
                JsonSerializer.Serialize(new[] { new { name = "objeto", value = "Contrato 2026/0481" } }),
                "[]");
        }

        _group.MarkQuoted();
        _group.SelectQuotation(_quotation.Id);

        // A emissão carrega o Grupo COM a réplica do endereço (RN-503).
        _groupRepository.GetByIdWithInsuredAddressAsync(_group.Id, Arg.Any<CancellationToken>()).Returns(_group);
        _quotationRepository.GetByIdAsync(_quotation.Id, Arg.Any<CancellationToken>()).Returns(_quotation);
        _enablementRepository.GetByIdAsync(_enablement.Id, Arg.Any<CancellationToken>()).Returns(_enablement);
        _policyRepository.ExistsForQuotationAsync(_quotation.Id, Arg.Any<CancellationToken>()).Returns(false);
        _personRepository.GetByIdAsync(_brokerageId, Arg.Any<CancellationToken>())
            .Returns(Person.Create("12345678000195", "Corretora", null, Guid.CreateVersion7()));
        _insurerRepository.GetByIdAsync(_insurerId, Arg.Any<CancellationToken>())
            .Returns(Insurer.Create("11222333000181", "Seguradora X", null, null, EInsurerStatus.Active, "REF-1"));
        _currentUser.UserIdentifier.Returns(ExternalIdentity);
        _userRepository.GetByExternalIdentityAsync(ExternalIdentity, Arg.Any<CancellationToken>())
            .Returns(User.Create("Diego", "diego@onpoint.com.br", ExternalIdentity));
        _registerTermAcceptance.ExecuteAsync(
                Arg.Any<RegisterTermAcceptanceRequest>(), Arg.Any<CancellationToken>())
            .Returns(new RegisterTermAcceptanceResponse
            {
                TermAcceptanceId = Guid.CreateVersion7(),
                AcceptedAt = DateTime.UtcNow,
            });

        return new RequestPolicyIssuanceUseCase(
            _groupRepository, _quotationRepository, _policyRepository, _enablementRepository,
            _personRepository, _insurerRepository, _registerTermAcceptance, _userRepository,
            _importedModalityRepository, _tagRepository,
            _currentUser, _unitOfWork, services.BuildServiceProvider());
    }

    private RequestPolicyIssuanceRequest Request(
        bool termAccepted = true, int installmentNumber = 1, int gracePeriodInDays = 30)
        => new()
        {
            QuotationGroupId = _group.Id,
            InstallmentNumber = installmentNumber,
            GracePeriodInDays = gracePeriodInDays,
            TermAccepted = termAccepted,
            UserAgent = "Mozilla/5.0",
        };

    private async Task AssertBlockedWithoutTouchingInsurer(Func<Task> act, string expectedMessagePart)
    {
        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage($"*{expectedMessagePart}*");

        await _engine.DidNotReceive().CreatePolicyAsync(
            Arg.Any<string?>(), Arg.Any<CreatePolicyInput>(), Arg.Any<CancellationToken>());
        await _engine.DidNotReceive().SubmitProposalTermsAsync(
            Arg.Any<string?>(), Arg.Any<SubmitProposalTermsInput>(), Arg.Any<CancellationToken>());
        await _policyRepository.DidNotReceive().AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
        _group.Status.Should().Be(EQuotationGroupStatus.Quoted);
    }

    [Fact]
    [Trait("RuleId", "RN-501")]
    public async Task Execute_ContragarantiaExigidaSemAssinatura_DeveBloquearSemAcionarSeguradora()
    {
        var useCase = BuildUseCase(requiresCcg: true, ccgSigned: false);

        await AssertBlockedWithoutTouchingInsurer(
            () => useCase.ExecuteAsync(Request(), CancellationToken.None), "Contragarantia");
    }

    [Fact]
    [Trait("RuleId", "RN-501")]
    public async Task Execute_ContragarantiaExigidaComAssinatura_NaoDeveBloquear()
    {
        var useCase = BuildUseCase(requiresCcg: true, ccgSigned: true);
        _engine.CreatePolicyAsync(Arg.Any<string?>(), Arg.Any<CreatePolicyInput>(), Arg.Any<CancellationToken>())
            .Returns(new PolicyIssuanceResult { PolicyExternalId = "AP-1", ProposalNumber = "PROP-1" });

        await useCase.ExecuteAsync(Request(), CancellationToken.None);

        await _engine.Received(1).CreatePolicyAsync(
            Arg.Any<string?>(), Arg.Any<CreatePolicyInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-502")]
    public async Task Execute_MinutaSemPreenchimento_DeveBloquearSemAcionarSeguradora()
    {
        var useCase = BuildUseCase(withMinuta: false);

        await AssertBlockedWithoutTouchingInsurer(
            () => useCase.ExecuteAsync(Request(), CancellationToken.None), "minuta");
    }

    /// <summary>
    /// RN-502 (caso limite): "Cotação cuja Seguradora não oferece Tag alguma: nada a preencher, segue
    /// direto". Sem catálogo importado para a Seguradora/Modalidade não há minuta a exigir — bloquear
    /// aqui impediria emitir qualquer oferta cuja Modalidade não define Tag.
    /// </summary>
    [Fact]
    [Trait("RuleId", "RN-502")]
    public async Task Execute_ModalidadeSemTagNoCatalogo_NaoDeveExigirMinuta()
    {
        var useCase = BuildUseCase(withMinuta: false);
        _importedModalityRepository
            .GetActiveByInsurerAndModalityAsync(_insurerId, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((ImportedModality?)null);
        _engine.CreatePolicyAsync(Arg.Any<string?>(), Arg.Any<CreatePolicyInput>(), Arg.Any<CancellationToken>())
            .Returns(new PolicyIssuanceResult { PolicyExternalId = "AP-1", ProposalNumber = "PROP-1" });

        await useCase.ExecuteAsync(Request(), CancellationToken.None);

        await _engine.Received(1).CreatePolicyAsync(
            Arg.Any<string?>(), Arg.Any<CreatePolicyInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-506")]
    public async Task Execute_TermoNaoAceito_DeveBloquearSemAcionarSeguradora()
    {
        var useCase = BuildUseCase();

        await AssertBlockedWithoutTouchingInsurer(
            () => useCase.ExecuteAsync(Request(termAccepted: false), CancellationToken.None), "Termo");
    }

    [Theory]
    [InlineData(4, 30)]
    [InlineData(1, 45)]
    [Trait("RuleId", "RN-505")]
    public async Task Execute_PagamentoForaDasOpcoesDaCotacao_DeveBloquearSemAcionarSeguradora(
        int installmentNumber, int gracePeriodInDays)
    {
        var useCase = BuildUseCase();

        await AssertBlockedWithoutTouchingInsurer(
            () => useCase.ExecuteAsync(
                Request(installmentNumber: installmentNumber, gracePeriodInDays: gracePeriodInDays),
                CancellationToken.None),
            "seguradora");
    }

    [Fact]
    [Trait("RuleId", "RN-507")]
    public async Task Execute_ApoliceJaRegistradaParaACotacao_DeveBloquearSemAcionarSeguradora()
    {
        var useCase = BuildUseCase();
        _policyRepository.ExistsForQuotationAsync(_quotation.Id, Arg.Any<CancellationToken>()).Returns(true);

        await AssertBlockedWithoutTouchingInsurer(
            () => useCase.ExecuteAsync(Request(), CancellationToken.None), "já foi solicitada");
    }

    [Fact]
    [Trait("RuleId", "RN-500")]
    public async Task Execute_CotacaoEmAnaliseDeSubscricao_DeveBloquearComMotivoProprio()
    {
        var useCase = BuildUseCase(result: EQuotationResult.Analysis);

        await AssertBlockedWithoutTouchingInsurer(
            () => useCase.ExecuteAsync(Request(), CancellationToken.None), "análise");
    }

    [Fact]
    [Trait("RuleId", "RN-503")]
    public async Task Execute_SemEnderecoDoSeguradoNaOferta_DeveBloquearSemAcionarSeguradora()
    {
        var useCase = BuildUseCase(withInsuredAddress: false);

        await AssertBlockedWithoutTouchingInsurer(
            () => useCase.ExecuteAsync(Request(), CancellationToken.None), "endereço");
    }

    [Fact]
    [Trait("RuleId", "RN-500")]
    public async Task Execute_SemCotacaoEscolhida_DeveBloquear()
    {
        var useCase = BuildUseCase();
        _group.ClearSelection();

        await AssertBlockedWithoutTouchingInsurer(
            () => useCase.ExecuteAsync(Request(), CancellationToken.None), "cotação");
    }
}
