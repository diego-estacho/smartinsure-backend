using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SmartInsure.Application.UseCase.Services.Quotations;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.Services.Quotations;

/// <summary>RN-057 — Processor do fan-out: obtém e persiste a Cotação, tolerando falha isolada.</summary>
[Trait("RuleId", "RN-057")]
public class QuotationRequestProcessorTests
{
    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly IQuotationGroupRepository _groupRepository = Substitute.For<IQuotationGroupRepository>();
    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly IModalityRepository _modalityRepository = Substitute.For<IModalityRepository>();
    private readonly IInsurerRepository _insurerRepository = Substitute.For<IInsurerRepository>();
    private readonly IBrokerageInsurerEnablementRepository _enablementRepository =
        Substitute.For<IBrokerageInsurerEnablementRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICalculationEngine _engine = Substitute.For<ICalculationEngine>();

    private readonly Guid _brokerageId = Guid.CreateVersion7();
    private readonly Guid _insurerId = Guid.CreateVersion7();
    private QuotationGroup _group = null!;
    private Quotation _quotation = null!;

    private QuotationRequestProcessor BuildProcessor()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<ICalculationEngine>(ECalculationEngine.PlugV2, (_, _) => _engine);

        return new QuotationRequestProcessor(
            _quotationRepository, _groupRepository, _personRepository, _modalityRepository,
            _insurerRepository, _enablementRepository, _unitOfWork, services.BuildServiceProvider());
    }

    /// <summary>Raiz de CNPJ compartilhada pela matriz (Tomador) e pela Filial nos cenários de RN-102.</summary>
    private const string MatrizCnpj = "11444777000161";
    private const string FilialCnpj = "11444777000323";

    /// <summary>Monta o grafo de dados válido (grupo, habilitação ativa, seguradora ativa, modalidade global, pessoas).</summary>
    private void SetupValidGraph(EInsurerStatus insurerStatus = EInsurerStatus.Active, Guid? branchPersonId = null)
    {
        var policyHolderId = Guid.CreateVersion7();
        var insuredId = Guid.CreateVersion7();
        var modalityId = Guid.CreateVersion7();

        _group = QuotationGroup.Create(
            policyHolderId, branchPersonId, insuredId, modalityId, 100_000m,
            new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            EQuotationScopeMode.All, [], includesPenaltyCoverage: false, includesLaborCoverage: false);

        _quotation = Quotation.Requested(_group.Id, _insurerId);

        _quotationRepository.GetByIdAsync(_quotation.Id, Arg.Any<CancellationToken>()).Returns(_quotation);
        _groupRepository.GetByIdAsync(_group.Id, Arg.Any<CancellationToken>()).Returns(_group);
        _enablementRepository.GetByPairAsync(_brokerageId, _insurerId, Arg.Any<CancellationToken>())
            .Returns(BrokerageInsurerEnablement.Create(
                _brokerageId, _insurerId, ECalculationEngine.PlugV2, "{\"baseUrl\":\"https://x/\",\"key\":\"k\"}"));
        _insurerRepository.GetByIdAsync(_insurerId, Arg.Any<CancellationToken>())
            .Returns(Insurer.Create("11222333000181", "Seguradora X", null, null, insurerStatus, "REF-123"));
        _modalityRepository.GetByIdAsync(modalityId, Arg.Any<CancellationToken>())
            .Returns(Modality.CreateFromGlobal("31", "Judicial"));

        var legalNatureId = Guid.CreateVersion7();
        _personRepository.GetByIdAsync(_brokerageId, Arg.Any<CancellationToken>())
            .Returns(Person.Create("12345678000195", "Corretora", null, legalNatureId));
        _personRepository.GetByIdAsync(policyHolderId, Arg.Any<CancellationToken>())
            .Returns(Person.Create(MatrizCnpj, "Tomador", null, legalNatureId));
        _personRepository.GetByIdAsync(insuredId, Arg.Any<CancellationToken>())
            .Returns(Person.Create("11444777000242", "Segurado", null, legalNatureId));
    }

    private QuotationRequestWorkItem WorkItem()
        => new(_quotation.Id, _group.Id, _insurerId, _brokerageId);

    [Fact]
    public async Task Process_DeveObterEGravarAutomatic_QuandoMotorResponde()
    {
        SetupValidGraph();
        _engine.RunQuotationAsync(Arg.Any<string?>(), Arg.Any<QuotationRequestInput>(), Arg.Any<CancellationToken>())
            .Returns(new QuotationResult { Result = EQuotationResult.Automatic, Premium = 300m });

        await BuildProcessor().ProcessAsync(WorkItem(), CancellationToken.None);

        _quotation.ProcessingStatus.Should().Be(EQuotationProcessingStatus.Obtained);
        _quotation.Result.Should().Be(EQuotationResult.Automatic);
        _quotation.Premium.Should().Be(300m);
        // Lease carimbado (ADR-050) + resultado: dois Update/Commit (lease antes do provedor, resultado depois).
        _quotation.ProcessingStartedAt.Should().NotBeNull();
        _quotationRepository.Received(2).Update(_quotation);
        await _unitOfWork.Received(2).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Process_DeveMarcarFalha_QuandoMotorLancaExcecao()
    {
        SetupValidGraph();
        _engine.RunQuotationAsync(Arg.Any<string?>(), Arg.Any<QuotationRequestInput>(), Arg.Any<CancellationToken>())
            .Throws(new CalculationEngineException("timeout"));

        await BuildProcessor().ProcessAsync(WorkItem(), CancellationToken.None);

        _quotation.ProcessingStatus.Should().Be(EQuotationProcessingStatus.Failed);
        _quotation.Result.Should().Be(EQuotationResult.Unavailable);
        _quotation.Reasons.Should().NotBeEmpty();
        // Lease (antes do provedor) + falha (depois): dois commits.
        await _unitOfWork.Received(2).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Process_DeveMarcarFalhaSemChamarMotor_QuandoSeguradoraInativa()
    {
        SetupValidGraph(insurerStatus: EInsurerStatus.Inactive);

        await BuildProcessor().ProcessAsync(WorkItem(), CancellationToken.None);

        _quotation.ProcessingStatus.Should().Be(EQuotationProcessingStatus.Failed);
        _quotation.Result.Should().Be(EQuotationResult.Unavailable);
        await _engine.DidNotReceive().RunQuotationAsync(
            Arg.Any<string?>(), Arg.Any<QuotationRequestInput>(), Arg.Any<CancellationToken>());
        // Lease (antes do provedor) + falha de pré-condição (depois): dois commits.
        await _unitOfWork.Received(2).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Process_DeveIgnorar_QuandoCotacaoNaoEstaMaisRequested()
    {
        SetupValidGraph();
        // Já obtida (não mais Requested) — item duplicado/idempotência: nada a fazer.
        _quotation.MarkObtained(
            EQuotationResult.Automatic, null, 300m, null, null, null, null, null, null,
            false, null, false, [], new DateTime(2026, 7, 28, 12, 0, 0, DateTimeKind.Utc));

        await BuildProcessor().ProcessAsync(WorkItem(), CancellationToken.None);

        await _engine.DidNotReceive().RunQuotationAsync(
            Arg.Any<string?>(), Arg.Any<QuotationRequestInput>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-102")]
    public async Task Process_DeveEnviarCnpjDaFilial_QuandoGrupoTemFilialMarcada()
    {
        var branchId = Guid.CreateVersion7();
        SetupValidGraph(branchPersonId: branchId);
        var legalNatureId = Guid.CreateVersion7();
        _personRepository.GetByIdAsync(branchId, Arg.Any<CancellationToken>())
            .Returns(Person.Create(FilialCnpj, "Filial", null, legalNatureId));

        QuotationRequestInput? captured = null;
        _engine.RunQuotationAsync(
                Arg.Any<string?>(), Arg.Do<QuotationRequestInput>(request => captured = request), Arg.Any<CancellationToken>())
            .Returns(new QuotationResult { Result = EQuotationResult.Automatic, Premium = 300m });

        await BuildProcessor().ProcessAsync(WorkItem(), CancellationToken.None);

        // Prova as duas pontas (RN-102): é o CNPJ da Filial e, explicitamente, não é o da matriz —
        // senão a asserção passaria por acidente caso os fixtures compartilhassem o mesmo documento.
        captured.Should().NotBeNull();
        captured!.PolicyHolderCnpj.Should().Be(FilialCnpj);
        captured.PolicyHolderCnpj.Should().NotBe(MatrizCnpj);
    }

    [Fact]
    [Trait("RuleId", "RN-102")]
    public async Task Process_DeveEnviarCnpjDaMatriz_QuandoGrupoSemFilialMarcada()
    {
        SetupValidGraph(branchPersonId: null);

        QuotationRequestInput? captured = null;
        _engine.RunQuotationAsync(
                Arg.Any<string?>(), Arg.Do<QuotationRequestInput>(request => captured = request), Arg.Any<CancellationToken>())
            .Returns(new QuotationResult { Result = EQuotationResult.Automatic, Premium = 300m });

        await BuildProcessor().ProcessAsync(WorkItem(), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.PolicyHolderCnpj.Should().Be(MatrizCnpj);
        // Sem Filial marcada, nenhuma Pessoa adicional é consultada — só Corretora, Tomador e Segurado.
        await _personRepository.Received(3).GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-102")]
    public async Task Process_DeveMarcarFalhaSemChamarMotor_QuandoFilialMarcadaNaoEncontrada()
    {
        var branchId = Guid.CreateVersion7();
        SetupValidGraph(branchPersonId: branchId);
        // Filial referenciada não é encontrada: personRepository devolve null (padrão do NSubstitute) para o id.

        await BuildProcessor().ProcessAsync(WorkItem(), CancellationToken.None);

        _quotation.ProcessingStatus.Should().Be(EQuotationProcessingStatus.Failed);
        _quotation.Result.Should().Be(EQuotationResult.Unavailable);
        await _engine.DidNotReceive().RunQuotationAsync(
            Arg.Any<string?>(), Arg.Any<QuotationRequestInput>(), Arg.Any<CancellationToken>());
    }
}
