namespace SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Responses;

/// <summary>Mapa de Modalidades (RN-033/RN-034): a matriz curada e a Fila de pendências.</summary>
public sealed record ModalityMapResponse(
    IReadOnlyList<ModalityMapEntryResponse> Modalities,
    IReadOnlyList<PendingImportedModalityResponse> Pending);

/// <summary>Uma Modalidade no Mapa com as Seguradoras que a oferecem (mapeamento Confirmado ativo).</summary>
public sealed record ModalityMapEntryResponse(
    Guid ModalityId,
    string Name,
    string GroupName,
    string Status,
    bool Offered,
    IReadOnlyList<string> Branches,
    IReadOnlyList<MapInsurerResponse> Insurers);

public sealed record MapInsurerResponse(
    Guid InsurerId,
    string InsurerName,
    Guid ImportedModalityId,
    string OriginName);

/// <summary>Item da Fila de Revisão — pendência a resolver (RN-034).</summary>
public sealed record PendingImportedModalityResponse(
    Guid ImportedModalityId,
    Guid InsurerId,
    string InsurerName,
    string OriginName,
    string Branch,
    string? EngineModalityName,
    string GroupName);
