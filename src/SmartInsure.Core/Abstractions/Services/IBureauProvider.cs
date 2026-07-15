using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Abstractions.Services;

/// <summary>
/// Contrato do Bureau (glossário) — implementado na camada de integração.
/// RN-003: toda solicitação gera uma nova consulta na fonte, sem reuso de resposta.
/// RN-004: falha ou indisponibilidade retorna consulta sem dado (null), nunca
/// exceção que bloqueie o fluxo solicitante.
/// </summary>
public interface IBureauProvider
{
    /// <summary>
    /// Consulta os dados cadastrais do CPF/CNPJ no bureau informado.
    /// Retorna null quando a fonte falha, está indisponível ou responde sem dado (RN-003/RN-004).
    /// </summary>
    /// <param name="cpfCnpj">Documento consultado, somente dígitos.</param>
    /// <param name="personType">Tipo de pessoa no contexto da solicitação (ex.: Segurado), definido pelo chamador.</param>
    /// <param name="bureau">Bureau homologado que responde a consulta.</param>
    Task<BureauPersonComplement?> GetPersonComplementAsync(
        string cpfCnpj,
        string personType,
        EBureau bureau,
        CancellationToken cancellationToken);
}
