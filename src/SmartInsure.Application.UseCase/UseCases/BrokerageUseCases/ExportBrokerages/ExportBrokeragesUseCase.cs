using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ExportBrokerages.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ExportBrokerages.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ExportBrokerages.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ExportBrokerages;

/// <summary>
/// RN-018 — exporta a listagem de Corretoras para .xlsx (v1 síncrona) aplicando os mesmos
/// filtros da listagem, com teto de segurança de 10.000 linhas.
/// </summary>
public sealed class ExportBrokeragesUseCase(
    IPersonRepository personRepository,
    IExcelExporter excelExporter) : IExportBrokeragesUseCase
{
    private const int ExportRowCap = 10000;

    private const string XlsxContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public async Task<ExportBrokeragesResponse> ExecuteAsync(
        ExportBrokeragesRequest request,
        CancellationToken cancellationToken)
    {
        var query = new BrokerageListQuery(
            1,
            ExportRowCap,
            request.Search,
            ParseSituation(request.Situation),
            request.InsurerId,
            ParseCalculationEngine(request.CalculationEngine),
            ParseSector(request.Sector),
            request.RegisteredFrom?.Date,
            request.RegisteredTo?.Date.AddDays(1).AddTicks(-1));

        var result = await personRepository.ListBrokeragesAsync(query, cancellationToken);

        IReadOnlyList<ExcelColumn<BrokerageListItemDto>> columns =
        [
            new("CNPJ", item => FormatCnpj(item.DocumentNumber)),
            new("Razão social", item => item.Name),
            new("Nome fantasia", item => item.SocialName ?? ""),
            new("Situação", item => SituationLabel(item.Situation)),
            new("Seguradoras habilitadas", item => item.EnabledInsurerCount),
            new("Seguradoras", item => string.Join(", ", item.EnabledInsurerNames)),
            new("Motor de cálculo", item => string.Join(", ", item.CalculationEngines)),
            new("Data de cadastro", item => item.RegisteredAt),
        ];

        var content = excelExporter.Export(result.Items, columns, "Corretoras");

        return new ExportBrokeragesResponse(content, "corretoras.xlsx", XlsxContentType);
    }

    private static string FormatCnpj(string documentNumber)
    {
        var digits = new string([.. documentNumber.Where(char.IsDigit)]);

        return digits.Length == 14
            ? $"{digits[..2]}.{digits[2..5]}.{digits[5..8]}/{digits[8..12]}-{digits[12..14]}"
            : documentNumber;
    }

    private static string SituationLabel(string situation) => situation switch
    {
        "Active" => "Ativa",
        "Incomplete" => "Incompleta",
        "Inactive" => "Inativa",
        _ => situation,
    };

    private static EBrokerageSituation? ParseSituation(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<EBrokerageSituation>(value, ignoreCase: true, out var parsed)
            ? parsed
            : throw new BusinessRuleException("A situação deve ser Active, Incomplete ou Inactive.");
    }

    private static ECalculationEngine? ParseCalculationEngine(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<ECalculationEngine>(value, ignoreCase: true, out var parsed)
            ? parsed
            : throw new BusinessRuleException("Motor de cálculo inválido.");
    }

    private static bool? ParseSector(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "private" or "privado" => true,
            "public" or "publico" or "público" => false,
            _ => throw new BusinessRuleException("O setor deve ser Public ou Private."),
        };
    }
}
