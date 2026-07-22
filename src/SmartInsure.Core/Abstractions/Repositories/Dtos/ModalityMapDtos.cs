namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>
/// Vínculo ativo Modalidade → Seguradora (RN-033): uma linha por Modalidade Importada Ativa,
/// não Ignorada, com <c>ModalityId</c>. O Mapa agrega por Seguradora distinta (ADR-061).
/// </summary>
public sealed record ModalityInsurerLinkDto(
    Guid ModalityId,
    Guid InsurerId,
    string InsurerName,
    string OriginName,
    string Branch);

/// <summary>
/// Pendência da Fila de Revisão (RN-034): Modalidade Importada Ativa, não Ignorada, sem vínculo
/// (<c>ModalityId</c> nulo) — exceção sem id de Modalidade Global.
/// </summary>
public sealed record PendingImportedModalityDto(
    Guid ImportedModalityId,
    Guid InsurerId,
    string InsurerName,
    string OriginName,
    string Branch,
    string? EngineModalityName,
    string GroupName);
