using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Abstractions.Services;

/// <summary>
/// Contrato do Motor de Cálculo para importação de modalidades (RN-031). Tipos do domínio/contrato,
/// nunca do fornecedor — a tradução do payload do parceiro fica na ACL do provider (ADR-045).
/// </summary>
public sealed record ImportedCatalogResult(IReadOnlyList<ImportedInsurerCatalog> Insurers);

/// <summary>Catálogo de uma Seguradora no retorno do motor; `IsSuccess` isola a falha por Seguradora (RN-035).</summary>
public sealed record ImportedInsurerCatalog(
    string InsuranceReferenceExternalId,
    string InsuranceName,
    bool IsSuccess,
    IReadOnlyList<ImportedModalityData> Modalities);

/// <summary>Uma Modalidade Importada trazida pelo motor, já normalizada para o contrato do domínio.</summary>
public sealed record ImportedModalityData(
    string SourceId,
    string OriginName,
    ESuretyBranch Branch,
    string? EngineModalityId,
    string? EngineModalityName,
    string GroupSourceId,
    string GroupName,
    string? GroupType,
    string RawParameters);
