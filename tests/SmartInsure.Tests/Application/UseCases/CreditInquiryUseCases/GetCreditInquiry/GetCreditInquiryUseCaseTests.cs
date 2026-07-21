using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Responses;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.GetCreditInquiry;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.GetCreditInquiry.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.CreditInquiryUseCases.GetCreditInquiry;

/// <summary>RN-031 — Recuperação do histórico de uma Consulta de Crédito.</summary>
[Trait("RuleId", "RN-031")]
public class GetCreditInquiryUseCaseTests
{
    private static readonly Guid BrokerageId = Guid.CreateVersion7();
    private static readonly Guid Insurer1Id = Guid.CreateVersion7();
    private static readonly Guid Insurer2Id = Guid.CreateVersion7();
    private static readonly string ValidCnpj = "12345678000195";

    private readonly ICreditInquiryRepository _creditInquiryRepository =
        Substitute.For<ICreditInquiryRepository>();

    private readonly IInsurerRepository _insurerRepository =
        Substitute.For<IInsurerRepository>();

    private readonly GetCreditInquiryUseCase _useCase;

    public GetCreditInquiryUseCaseTests()
    {
        _useCase = new GetCreditInquiryUseCase(_creditInquiryRepository, _insurerRepository);
    }

    private static Insurer CreateInsurer(Guid id, string corporateName)
    {
        var insurer = Insurer.Create(
            "98765432000109", corporateName, null, null,
            EInsurerStatus.Active, "InsurerId123");

        // Manual Id assignment for testing (since Id is private)
        typeof(EntityBase).GetProperty("Id")!.SetValue(insurer, id);
        return insurer;
    }

    [Fact]
    public async Task Execute_DeveRetornarConsultaComResultados_QuandoConsultaExiste()
    {
        var inquiryId = Guid.CreateVersion7();
        var inquiry = CreditInquiry.Create(BrokerageId, ValidCnpj);
        typeof(EntityBase).GetProperty("Id")!.SetValue(inquiry, inquiryId);

        var availableResult = CreditInquiryResult.Available(
            inquiryId, Insurer1Id,
            1000m, 0.05m, 2000m, 0.06m, 0.07m, 3000m, 0.08m,
            DateTime.UtcNow.AddMonths(12));

        inquiry.AddResult(availableResult);

        _creditInquiryRepository.GetByIdAsync(inquiryId, Arg.Any<CancellationToken>())
            .Returns(inquiry);

        var insurer1 = CreateInsurer(Insurer1Id, "Seguradora Beta S.A.");
        _insurerRepository.GetByIdAsync(Insurer1Id, Arg.Any<CancellationToken>())
            .Returns(insurer1);

        var request = new GetCreditInquiryRequest(inquiryId);
        var response = await _useCase.ExecuteAsync(request, CancellationToken.None);

        response.CreditInquiryId.Should().Be(inquiryId);
        response.PolicyHolderCnpj.Should().Be(ValidCnpj);
        response.Results.Should().HaveCount(1);
        response.Summary.InsurersQueried.Should().Be(1);
        response.Summary.InsurersAvailable.Should().Be(1);
    }

    [Fact]
    public async Task Execute_DeveCalcularConsolidadoComoMaximoModalidadePorSeguradora_QuandoVariasModalidadesDisponiveis()
    {
        var inquiryId = Guid.CreateVersion7();
        var inquiry = CreditInquiry.Create(BrokerageId, ValidCnpj);
        typeof(EntityBase).GetProperty("Id")!.SetValue(inquiry, inquiryId);

        // Insurer 1: Traditional=1000, Judicial=2000, Financial=500 -> max=2000
        var result1 = CreditInquiryResult.Available(
            inquiryId, Insurer1Id,
            1000m, 0.05m, 2000m, 0.06m, 0.07m, 500m, 0.08m,
            DateTime.UtcNow.AddMonths(12));

        // Insurer 2: Traditional=3000, Judicial=1500, Financial=2500 -> max=3000
        var result2 = CreditInquiryResult.Available(
            inquiryId, Insurer2Id,
            3000m, 0.05m, 1500m, 0.06m, 0.07m, 2500m, 0.08m,
            DateTime.UtcNow.AddMonths(12));

        inquiry.AddResult(result1);
        inquiry.AddResult(result2);

        _creditInquiryRepository.GetByIdAsync(inquiryId, Arg.Any<CancellationToken>())
            .Returns(inquiry);

        var insurer1 = CreateInsurer(Insurer1Id, "Seguradora Alfa");
        var insurer2 = CreateInsurer(Insurer2Id, "Seguradora Beta");
        _insurerRepository.GetByIdAsync(Insurer1Id, Arg.Any<CancellationToken>())
            .Returns(insurer1);
        _insurerRepository.GetByIdAsync(Insurer2Id, Arg.Any<CancellationToken>())
            .Returns(insurer2);

        var request = new GetCreditInquiryRequest(inquiryId);
        var response = await _useCase.ExecuteAsync(request, CancellationToken.None);

        // 2000 (insurer1 max) + 3000 (insurer2 max) = 5000
        response.Summary.ConsolidatedLimit.Should().Be(5000m);
        response.Summary.InsurersAvailable.Should().Be(2);
    }

    [Fact]
    public async Task Execute_DeveIncluirResultadosIndisponiveisComMotivo_QuandoAlgumaSeguradoraNaoDisponivel()
    {
        var inquiryId = Guid.CreateVersion7();
        var inquiry = CreditInquiry.Create(BrokerageId, ValidCnpj);
        typeof(EntityBase).GetProperty("Id")!.SetValue(inquiry, inquiryId);

        var availableResult = CreditInquiryResult.Available(
            inquiryId, Insurer1Id,
            1000m, 0.05m, null, null, null, null, null,
            DateTime.UtcNow.AddMonths(6));

        var unavailableResult = CreditInquiryResult.Unavailable(
            inquiryId, Insurer2Id,
            "Sistema indisponível");

        inquiry.AddResult(availableResult);
        inquiry.AddResult(unavailableResult);

        _creditInquiryRepository.GetByIdAsync(inquiryId, Arg.Any<CancellationToken>())
            .Returns(inquiry);

        var insurer1 = CreateInsurer(Insurer1Id, "Seguradora Disponível");
        _insurerRepository.GetByIdAsync(Insurer1Id, Arg.Any<CancellationToken>())
            .Returns(insurer1);
        _insurerRepository.GetByIdAsync(Insurer2Id, Arg.Any<CancellationToken>())
            .Returns((Insurer?)null); // Simulates insurer not found

        var request = new GetCreditInquiryRequest(inquiryId);
        var response = await _useCase.ExecuteAsync(request, CancellationToken.None);

        response.Results.Should().HaveCount(2);
        response.Results.Should().ContainSingle(r => r.Status == "Available");
        response.Results.Should().ContainSingle(r => r.Status == "Unavailable" && r.FailureReason == "Sistema indisponível");
        response.Summary.InsurersAvailable.Should().Be(1);
    }

    [Fact]
    public async Task Execute_DeveUsarNomePadrao_QuandoSeguradoraNaoEncontradaNoRepositorio()
    {
        var inquiryId = Guid.CreateVersion7();
        var inquiry = CreditInquiry.Create(BrokerageId, ValidCnpj);
        typeof(EntityBase).GetProperty("Id")!.SetValue(inquiry, inquiryId);

        var result = CreditInquiryResult.Available(
            inquiryId, Insurer1Id,
            1000m, 0.05m, null, null, null, null, null,
            DateTime.UtcNow.AddMonths(6));

        inquiry.AddResult(result);

        _creditInquiryRepository.GetByIdAsync(inquiryId, Arg.Any<CancellationToken>())
            .Returns(inquiry);

        _insurerRepository.GetByIdAsync(Insurer1Id, Arg.Any<CancellationToken>())
            .Returns((Insurer?)null);

        var request = new GetCreditInquiryRequest(inquiryId);
        var response = await _useCase.ExecuteAsync(request, CancellationToken.None);

        response.Results.Single().InsurerName.Should().Be("Seguradora desconhecida");
    }

    [Fact]
    public async Task Execute_DeveLancarNotFoundException_QuandoConsultaNaoEncontrada()
    {
        var inquiryId = Guid.CreateVersion7();

        _creditInquiryRepository.GetByIdAsync(inquiryId, Arg.Any<CancellationToken>())
            .Returns((CreditInquiry?)null);

        var request = new GetCreditInquiryRequest(inquiryId);
        var act = () => _useCase.ExecuteAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Consulta de crédito não encontrada*");
    }
}
