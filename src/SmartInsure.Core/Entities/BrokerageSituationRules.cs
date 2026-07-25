using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Entities;

/// <summary>
/// RN-033 — fonte única da regra da situação apresentada da Corretora (derivada).
/// A Corretora com papel Inativo é sempre Inativa; com papel Ativo, é Ativa quando o cadastro
/// está completo (nome fantasia e e-mail de contato presentes) e Incompleta quando falta um deles.
/// A mesma regra vale na listagem, na contagem, no filtro e no detalhe (calculada no servidor).
/// </summary>
public static class BrokerageSituationRules
{
    public static EBrokerageSituation Resolve(
        EPersonRoleStatus status,
        string? socialName,
        string? contactEmail)
    {
        if (status == EPersonRoleStatus.Inactive)
        {
            return EBrokerageSituation.Inactive;
        }

        var complete = !string.IsNullOrWhiteSpace(socialName)
            && !string.IsNullOrWhiteSpace(contactEmail);

        return complete ? EBrokerageSituation.Active : EBrokerageSituation.Incomplete;
    }

    /// <summary>Completude do cadastro (nome fantasia e e-mail de contato presentes).</summary>
    public static bool IsComplete(string? socialName, string? contactEmail)
        => !string.IsNullOrWhiteSpace(socialName) && !string.IsNullOrWhiteSpace(contactEmail);
}
