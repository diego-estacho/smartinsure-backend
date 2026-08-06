using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExportCreditInquiry.Interfaces;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExportCreditInquiry.Requests;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExportCreditInquiry.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExportCreditInquiry;

/// <summary>
/// RN-201 — exporta o quadro consolidado de uma Consulta de Crédito para .xlsx, reusando o
/// <see cref="IExcelExporter"/> compartilhado (mesma engine da exportação de Corretoras, RN-018).
/// Uma linha por Seguradora, na mesma ordenação da tela (RN-029): Aprovado antes de Indisponível,
/// depois por maior limite disponível. Tomador, CNPJ e data ficam no nome do arquivo.
/// </summary>
public sealed class ExportCreditInquiryUseCase(
    ICreditInquiryRepository creditInquiryRepository,
    IInsurerRepository insurerRepository,
    IExcelExporter excelExporter) : IExportCreditInquiryUseCase
{
    private const string XlsxContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    // GroupTypes estáveis do Motor de Cálculo (RN-029) — espelham o mapeamento das colunas fixas
    // do front (lib/creditInquiry.ts): o judicial fiscal compõe a coluna Judicial.
    private const string TraditionalGroup = "GARANTIA_TRADICIONAL";
    private const string JudicialGroup = "GARANTIA_JUDICIAL";
    private const string JudicialFiscalGroup = "GARANTIA_JUDICIAL_FISCAL";
    private const string FinancialGroup = "GARANTIA_FINANCEIRA";

    public async Task<ExportCreditInquiryResponse> ExecuteAsync(
        ExportCreditInquiryRequest request,
        CancellationToken cancellationToken)
    {
        var inquiry = await creditInquiryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Consulta de crédito não encontrada.");

        var insurerIds = inquiry.Results.Select(result => result.InsurerId).Distinct().ToList();
        var insurerNames = await insurerRepository.GetCorporateNamesByIdsAsync(insurerIds, cancellationToken);

        // Mesma ordenação do quadro na tela (RN-029): Aprovado antes de Indisponível, depois por maior limite.
        var rows = inquiry.Results
            .Select(result => BuildRow(result, insurerNames))
            .OrderByDescending(row => row.IsAvailable)
            .ThenByDescending(row => row.MaxAvailable)
            .ToList();

        IReadOnlyList<ExcelColumn<CreditInquiryExportRow>> columns =
        [
            new("Seguradora", row => row.InsurerName),
            new("Status", row => row.Status),
            new("Limite Tradicional", row => row.TraditionalLimit),
            new("Taxa Tradicional (%)", row => row.TraditionalRate),
            new("Limite Judicial", row => row.JudicialLimit),
            new("Taxa Judicial (%)", row => row.JudicialRate),
            new("Limite Financeira", row => row.FinancialLimit),
            new("Taxa Financeira (%)", row => row.FinancialRate),
            new("Utilizado", row => row.Used),
            new("Tempo de resposta (s)", row => row.ResponseTimeSeconds),
            new("Motivo", row => row.FailureReason),
        ];

        var content = excelExporter.Export(rows, columns, "Consulta de crédito");

        return new ExportCreditInquiryResponse(content, BuildFileName(inquiry.PolicyHolderCnpj), XlsxContentType);
    }

    private static CreditInquiryExportRow BuildRow(
        CreditInquiryResult result,
        IReadOnlyDictionary<Guid, string> insurerNames)
    {
        var insurerName = insurerNames.TryGetValue(result.InsurerId, out var name)
            ? name
            : "Seguradora desconhecida";

        var (traditionalLimit, traditionalRate) = Cell(FindGroup(result, TraditionalGroup));

        // Na ausência do judicial, o judicial fiscal assume a coluna (mesma composição do front).
        var (judicialLimit, judicialRate) = Cell(FindGroup(result, JudicialGroup) ?? FindGroup(result, JudicialFiscalGroup));

        var (financialLimit, financialRate) = Cell(FindGroup(result, FinancialGroup));

        var maxAvailable = result.Limits.Select(limit => limit.AvailableLimit).DefaultIfEmpty(0m).Max();

        var leader = result.Limits
            .OrderByDescending(limit => limit.AvailableLimit)
            .FirstOrDefault();
        var usedValue = leader is null ? 0m : Math.Max(0m, leader.RevisedLimit - leader.AvailableLimit);

        var responseSeconds = result.ResponseTimeMs is { } ms
            ? Math.Round(ms / 1000m, 1)
            : (decimal?)null;

        return new CreditInquiryExportRow(
            insurerName,
            result.Status == ECreditInquiryResultStatus.Available ? "Aprovado" : "Indisponível",
            result.Status == ECreditInquiryResultStatus.Available,
            maxAvailable,
            traditionalLimit,
            traditionalRate,
            judicialLimit,
            judicialRate,
            financialLimit,
            financialRate,
            usedValue > 0m ? usedValue : null,
            responseSeconds,
            result.FailureReason);
    }

    private static CreditInquiryResultLimit? FindGroup(CreditInquiryResult result, string groupType)
        => result.Limits.FirstOrDefault(limit => limit.GroupType == groupType);

    // Espelha o front: célula presente só quando o grupo existe e tem limite disponível > 0; senão ausente.
    private static (decimal? Limit, decimal? Rate) Cell(CreditInquiryResultLimit? group)
        => group is { AvailableLimit: > 0m }
            ? (group.AvailableLimit, group.Rate)
            : (null, null);

    private static string BuildFileName(string policyHolderCnpj)
    {
        var digits = new string([.. policyHolderCnpj.Where(char.IsDigit)]);

        return digits.Length > 0
            ? $"consulta-credito-{digits}.xlsx"
            : "consulta-credito.xlsx";
    }

    private sealed record CreditInquiryExportRow(
        string InsurerName,
        string Status,
        bool IsAvailable,
        decimal MaxAvailable,
        decimal? TraditionalLimit,
        decimal? TraditionalRate,
        decimal? JudicialLimit,
        decimal? JudicialRate,
        decimal? FinancialLimit,
        decimal? FinancialRate,
        decimal? Used,
        decimal? ResponseTimeSeconds,
        string? FailureReason);
}
