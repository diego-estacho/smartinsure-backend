using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Abstractions.Services;

/// <summary>
/// Resultado da consulta de Coberturas Adicionais de UMA Modalidade Importada ao Motor de Cálculo
/// (RN-041/RN-045). Tipos do domínio/contrato, nunca do fornecedor — a tradução do payload fica na
/// ACL do provider (ADR-045). `IsSuccess=false` (erro de envelope ou corpo nulo) é falha daquela
/// modalidade: não desativa suas coberturas importadas (RN-044/RN-045).
/// </summary>
public sealed record ImportedAdditionalCoverageResult(
    bool IsSuccess,
    IReadOnlyList<ImportedAdditionalCoverageData> Coverages,
    string? ErrorMessage);

/// <summary>Uma Cobertura Adicional Importada trazida pelo motor, já normalizada para o contrato do domínio (ramo mapeado).</summary>
public sealed record ImportedAdditionalCoverageData(
    string Name,
    string? SourceUniqueId,
    int InsuredAmountCalculationType,
    bool AllowManualEdit,
    ESuretyBranch Branch);
