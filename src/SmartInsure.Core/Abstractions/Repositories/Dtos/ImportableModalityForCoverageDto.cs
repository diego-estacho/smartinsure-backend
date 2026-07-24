using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>
/// Modalidade Importada processável na importação de Coberturas Adicionais (RN-042/RN-043):
/// Ativa e não Ignorada. Carrega o que a consulta à OnPoint exige por modalidade — nome de origem
/// e tipo do grupo importado — além do ramo, usado para confirmar a cobertura (RN-042).
/// </summary>
public sealed record ImportableModalityForCoverageDto(
    Guid ImportedModalityId,
    string ModalityName,
    string? ModalityGroupType,
    ESuretyBranch Branch);
