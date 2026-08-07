using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Application.UseCases.QuotationUseCases.ListQuotations;

/// <summary>
/// RN-505/RN-510 — a leitura do leque leva as opções de pagamento e os documentos exigidos que a
/// Seguradora informou: é dessa resposta que a etapa de emissão monta a forma de pagamento, sem
/// inventar opção nem acionar o provedor de novo.
/// </summary>
public class ListQuotationsPaymentOptionsTests
{
    private readonly IQuotationGroupRepository _groupRepository = Substitute.For<IQuotationGroupRepository>();
    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly IInsurerRepository _insurerRepository = Substitute.For<IInsurerRepository>();

    private readonly IAdditionalCoverageRepository _additionalCoverageRepository =
        Substitute.For<IAdditionalCoverageRepository>();

    private readonly Guid _insurerId = Guid.CreateVersion7();

    private ListQuotationsUseCase BuildUseCase(out QuotationGroup group)
    {
        group = QuotationGroup.Create(
            Guid.CreateVersion7(), null, Guid.CreateVersion7(), Guid.CreateVersion7(), 100_000m,
            new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            EQuotationScopeMode.All, [], []);

        var quotation = Quotation.Requested(group.Id, _insurerId);
        quotation.MarkObtained(
            EQuotationResult.ReadyForEmission, null, 300m, 20m, 60m, 1.5m, 500_000m,
            "prop-1", "PROP-1", false, null, false, [], DateTime.UtcNow);
        quotation.SetProviderOptions(
            [
                new QuotationInstallmentOption { Number = 1, Description = "À vista", Value = 300m, HasInterest = false },
                new QuotationInstallmentOption { Number = 3, Description = "3x", Value = 105m, HasInterest = true },
            ],
            [0, 30],
            [new QuotationRequiredDocument { Name = "Contrato social", Description = "Consolidado" }]);

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        _quotationRepository.ListByGroupAsync(group.Id, Arg.Any<CancellationToken>()).Returns([quotation]);
        _insurerRepository.GetCorporateNamesByIdsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string> { [_insurerId] = "Seguradora X" });
        _insurerRepository.GetLogoUrlsByIdsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string?>());
        _additionalCoverageRepository.GetNamesByIdsAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string>());

        return new ListQuotationsUseCase(
            _groupRepository, _quotationRepository, _insurerRepository, _additionalCoverageRepository);
    }

    [Fact]
    [Trait("RuleId", "RN-505")]
    public async Task Execute_DeveLevarAsOpcoesDeParcelamentoEOsVencimentosDaCotacao()
    {
        var useCase = BuildUseCase(out var group);

        var response = await useCase.ExecuteAsync(new ListQuotationsRequest(group.Id), CancellationToken.None);

        var item = response.Quotations.Single();
        item.InstallmentOptions.Should().HaveCount(2);
        item.InstallmentOptions[0].Number.Should().Be(1);
        item.InstallmentOptions[0].Description.Should().Be("À vista");
        item.InstallmentOptions[1].HasInterest.Should().BeTrue();
        item.PossibleGracePeriodsInDays.Should().Equal(0, 30);
    }

    [Fact]
    [Trait("RuleId", "RN-510")]
    public async Task Execute_DeveLevarOsDocumentosExigidosPelaSeguradora()
    {
        var useCase = BuildUseCase(out var group);

        var response = await useCase.ExecuteAsync(new ListQuotationsRequest(group.Id), CancellationToken.None);

        var item = response.Quotations.Single();
        item.RequiredDocuments.Should().ContainSingle();
        item.RequiredDocuments[0].Name.Should().Be("Contrato social");
        item.RequiredDocuments[0].Description.Should().Be("Consolidado");
    }
}
