namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>
/// RN-105: uma Cobertura Adicional Importada Ativa, vinculada a uma Cobertura Adicional canônica
/// escolhida, numa Modalidade Importada Ativa e não Ignorada da Seguradora cotada. O
/// <paramref name="Name"/> é o que vai à Seguradora (ADR-103).
/// </summary>
public sealed record OfferableImportedCoverageDto(
    Guid AdditionalCoverageId,
    Guid ImportedAdditionalCoverageId,
    string Name);

/// <summary>
/// RN-104: Cobertura Adicional canônica ofertável na etapa de risco para uma Modalidade — união
/// simples das Seguradoras habilitadas da Corretora do Escopo ativo.
/// </summary>
public sealed record AvailableAdditionalCoverageDto(Guid AdditionalCoverageId, string Name);
