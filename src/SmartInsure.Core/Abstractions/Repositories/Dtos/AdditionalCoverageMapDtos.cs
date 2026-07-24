namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>RN-040: Cobertura Adicional canônica no catálogo de curadoria.</summary>
public sealed record AdditionalCoverageListItemDto(Guid Id, string Name, string Status);

/// <summary>RN-043: Cobertura Adicional Importada pendente de mapeamento (Ativa, não Ignorada, sem vínculo).</summary>
public sealed record PendingImportedCoverageDto(
    Guid Id,
    Guid ImportedModalityId,
    string InsurerName,
    string ModalityName,
    string CoverageName);

/// <summary>RN-043/RN-046: Cobertura Adicional Importada Ativa vinculada a uma Cobertura Adicional canônica.</summary>
public sealed record LinkedImportedCoverageDto(
    Guid AdditionalCoverageId,
    Guid ImportedCoverageId,
    string InsurerName,
    string ModalityName,
    string CoverageName);
