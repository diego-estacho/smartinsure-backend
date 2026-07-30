using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ExportBrokerages;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ExportBrokerages.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Application.UseCases.BrokerageUseCases.ExportBrokerages;

/// <summary>RN-018 — Exportação da listagem de Corretoras (.xlsx síncrona, teto de 10.000 linhas).</summary>
[Trait("RuleId", "RN-018")]
public class ExportBrokeragesUseCaseTests
{
    private static readonly byte[] ExporterBytes = [1, 2, 3];

    private readonly IPersonRepository _repository = Substitute.For<IPersonRepository>();
    private readonly IExcelExporter _excelExporter = Substitute.For<IExcelExporter>();
    private readonly ExportBrokeragesUseCase _useCase;

    public ExportBrokeragesUseCaseTests()
    {
        _useCase = new ExportBrokeragesUseCase(_repository, _excelExporter);

        _repository.ListBrokeragesAsync(Arg.Any<BrokerageListQuery>(), Arg.Any<CancellationToken>())
            .Returns(new BrokerageListResult(
                [
                    new BrokerageListItemDto(
                        Guid.NewGuid(), "11444777000161", "Alfa Ltda", "Alfa", true,
                        "Active", "Active", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        2, ["Junto Seguros", "Pottencial"], ["PlugV2"]),
                    new BrokerageListItemDto(
                        Guid.NewGuid(), "12345678000195", "Beta SA", null, false,
                        "Inactive", "Inactive", new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc),
                        0, [], []),
                ],
                2L,
                new BrokerageSituationCountsDto(2, 1, 0, 1)));

        _excelExporter
            .Export(
                Arg.Any<IEnumerable<BrokerageListItemDto>>(),
                Arg.Any<IReadOnlyList<ExcelColumn<BrokerageListItemDto>>>(),
                Arg.Any<string>())
            .Returns(ExporterBytes);
    }

    [Fact]
    public async Task Execute_DeveConsultarComTetoDe10Mil_QuandoExporta()
    {
        await _useCase.ExecuteAsync(new ExportBrokeragesRequest(), CancellationToken.None);

        await _repository.Received(1).ListBrokeragesAsync(
            Arg.Is<BrokerageListQuery>(query => query.Page == 1 && query.PageSize == 10000),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveDevolverArquivoXlsxDoExporter_QuandoExporta()
    {
        var response = await _useCase.ExecuteAsync(new ExportBrokeragesRequest(), CancellationToken.None);

        response.FileName.Should().Be("corretoras.xlsx");
        response.ContentType.Should().Be(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        response.Content.Should().BeEquivalentTo(ExporterBytes);
    }

    [Fact]
    public async Task Execute_DeveExportarComOitoColunas_ParaAPlanilhaCorretoras()
    {
        await _useCase.ExecuteAsync(new ExportBrokeragesRequest(), CancellationToken.None);

        _excelExporter.Received(1).Export(
            Arg.Any<IEnumerable<BrokerageListItemDto>>(),
            Arg.Is<IReadOnlyList<ExcelColumn<BrokerageListItemDto>>>(columns => columns.Count == 8),
            "Corretoras");
    }

    [Fact]
    public async Task Execute_DeveMapearFiltros_IgualAListagem()
    {
        await _useCase.ExecuteAsync(
            new ExportBrokeragesRequest
            {
                Search = "alfa",
                Situation = "Incomplete",
                Sector = "Private",
                CalculationEngine = "PlugV2",
            },
            CancellationToken.None);

        await _repository.Received(1).ListBrokeragesAsync(
            Arg.Is<BrokerageListQuery>(query =>
                query.Search == "alfa"
                && query.Situation == EBrokerageSituation.Incomplete
                && query.IsPrivateSector == true
                && query.CalculationEngine == ECalculationEngine.PlugV2),
            Arg.Any<CancellationToken>());
    }
}
