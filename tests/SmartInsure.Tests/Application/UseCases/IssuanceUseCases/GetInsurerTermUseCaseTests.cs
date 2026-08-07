using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.GetInsurerTerm;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.GetInsurerTerm.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.IssuanceUseCases;

/// <summary>
/// RN-506 — a etapa de emissão apresenta o Termo e declaração VIGENTE da Seguradora da Cotação escolhida;
/// o texto vem do servidor, não do cliente, porque é o mesmo conteúdo que será registrado no aceite.
/// </summary>
[Trait("RuleId", "RN-506")]
public class GetInsurerTermUseCaseTests
{
    private readonly IQuotationGroupRepository _groupRepository = Substitute.For<IQuotationGroupRepository>();
    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly IInsurerTermRepository _termRepository = Substitute.For<IInsurerTermRepository>();

    private readonly Guid _insurerId = Guid.CreateVersion7();
    private const string TermContent = "O tomador declara ter lido e aceito as condições contratuais.";

    private QuotationGroup _group = null!;

    private GetInsurerTermUseCase BuildUseCase(bool withActiveTerm = true, bool withSelection = true)
    {
        _group = QuotationGroup.Create(
            Guid.CreateVersion7(), null, Guid.CreateVersion7(), Guid.CreateVersion7(), 100_000m,
            new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1),
            EQuotationScopeMode.All, [], []);

        var quotation = Quotation.Requested(_group.Id, _insurerId);
        quotation.MarkObtained(
            EQuotationResult.ReadyForEmission, null, 300m, 20m, 60m, 1.5m, 500_000m,
            "prop-1", "PROP-1", false, null, false, [], DateTime.UtcNow);

        _group.MarkQuoted();

        if (withSelection)
        {
            _group.SelectQuotation(quotation.Id);
        }

        _groupRepository.GetByIdAsync(_group.Id, Arg.Any<CancellationToken>()).Returns(_group);
        _quotationRepository.GetByIdAsync(quotation.Id, Arg.Any<CancellationToken>()).Returns(quotation);

        if (withActiveTerm)
        {
            _termRepository.GetActiveByInsurerAsync(_insurerId, Arg.Any<CancellationToken>())
                .Returns(InsurerTerm.Create(_insurerId, TermContent));
        }

        return new GetInsurerTermUseCase(_groupRepository, _quotationRepository, _termRepository);
    }

    [Fact]
    public async Task Execute_DeveDevolverOTextoVigenteDaSeguradoraDaCotacaoEscolhida()
    {
        var useCase = BuildUseCase();

        var response = await useCase.ExecuteAsync(
            new GetInsurerTermRequest(_group.Id), CancellationToken.None);

        response.Content.Should().Be(TermContent);
        response.InsurerId.Should().Be(_insurerId);
    }

    [Fact]
    public async Task Execute_SeguradoraSemTermoVigente_DeveRecusarComMotivo()
    {
        var useCase = BuildUseCase(withActiveTerm: false);

        var act = () => useCase.ExecuteAsync(new GetInsurerTermRequest(_group.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*Termo*");
    }

    [Fact]
    public async Task Execute_SemCotacaoEscolhida_DeveRecusar()
    {
        var useCase = BuildUseCase(withSelection: false);

        var act = () => useCase.ExecuteAsync(new GetInsurerTermRequest(_group.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
