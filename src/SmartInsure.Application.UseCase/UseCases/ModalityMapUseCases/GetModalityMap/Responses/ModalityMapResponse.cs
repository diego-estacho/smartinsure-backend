namespace SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Responses;

/// <summary>Mapa de Modalidades (RN-033/RN-034): a matriz e a Fila de exceções.</summary>
public sealed record ModalityMapResponse(
    IReadOnlyList<ModalityMapEntryResponse> Modalities,
    IReadOnlyList<PendingImportedModalityResponse> Pending);

/// <summary>Uma Modalidade no Mapa com as Seguradoras que a oferecem (uma entrada por Seguradora distinta).</summary>
public sealed record ModalityMapEntryResponse(
    Guid ModalityId,
    string Name,
    string Status,
    bool Offered,
    IReadOnlyList<string> Branches,
    IReadOnlyList<MapInsurerResponse> Insurers);

/// <summary>
/// Uma Seguradora distinta que oferece a Modalidade (RN-033): <c>Count</c> é quantas Modalidades
/// Importadas dessa Seguradora sustentam a Modalidade; <c>Origins</c> são os nomes de origem.
/// </summary>
public sealed record MapInsurerResponse(
    Guid InsurerId,
    string InsurerName,
    int Count,
    IReadOnlyList<string> Origins);

/// <summary>Item da Fila de Revisão — exceção a resolver (RN-034): Importada Ativa sem vínculo.</summary>
public sealed record PendingImportedModalityResponse(
    Guid ImportedModalityId,
    Guid InsurerId,
    string InsurerName,
    string OriginName,
    string Branch,
    string? EngineModalityName,
    string GroupName);
