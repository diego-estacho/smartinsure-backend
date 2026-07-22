namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>Mapeamento Confirmado ativo (RN-033): par Modalidade × Seguradora que a oferece.</summary>
public sealed record ConfirmedMappingDto(
    Guid ModalityId,
    Guid InsurerId,
    string InsurerName,
    Guid ImportedModalityId,
    string OriginName,
    string Branch);

/// <summary>Pendência da Fila de Revisão (RN-034): Modalidade Importada Ativa, não Ignorada, sem mapeamento Confirmado.</summary>
public sealed record PendingImportedModalityDto(
    Guid ImportedModalityId,
    Guid InsurerId,
    string InsurerName,
    string OriginName,
    string Branch,
    string? EngineModalityName,
    string GroupName);
