using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Mapeamento de Modalidade (RN-032/RN-033): liga uma Modalidade Importada à Modalidade (Smart)
/// que ela representa. É o que torna as ofertas comparáveis. Nesta fase só o automático
/// "por identificador do motor" é criado (Confirmado); semelhança está fora (OPEN-08) e o
/// manual/pendente é da Fila (RN-034, fatia 3).
/// </summary>
public sealed class ModalityMapping : EntityBase
{
    private ModalityMapping()
    {
    }

    public Guid ImportedModalityId { get; private set; }

    public Guid ModalityId { get; private set; }

    public EMappingEstablishment Establishment { get; private set; }

    /// <summary>Grau de confiança — só se aplica à forma `Similarity` (OPEN-08); nulo nas demais.</summary>
    public int? Confidence { get; private set; }

    public EModalityMappingStatus Status { get; private set; }

    public string? ConfirmedBy { get; private set; }

    public DateTime? ConfirmedAt { get; private set; }

    /// <summary>
    /// RN-032: mapeamento herdado pelo identificador do motor — sem ambiguidade, criado já Confirmado
    /// e sem autor humano (confirmação automática).
    /// </summary>
    public static ModalityMapping CreateByIdentifier(Guid importedModalityId, Guid modalityId)
        => new()
        {
            ImportedModalityId = importedModalityId,
            ModalityId = modalityId,
            Establishment = EMappingEstablishment.Identifier,
            Status = EModalityMappingStatus.Confirmed,
            Confidence = null,
            ConfirmedBy = null,
            ConfirmedAt = null,
        };
}
