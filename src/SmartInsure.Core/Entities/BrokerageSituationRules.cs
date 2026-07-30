using System.Linq.Expressions;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Entities;

/// <summary>
/// RN-102 — fonte única da regra da situação apresentada da Corretora (derivada).
/// A Corretora com papel Inativo é sempre Inativa; com papel Ativo, é Ativa quando o cadastro
/// está completo (nome fantasia e e-mail de contato presentes) e Incompleta quando falta um deles.
/// A mesma regra vale na listagem, na contagem, no filtro e no detalhe (calculada no servidor):
/// <see cref="Resolve"/> resolve em memória (projeção da página) e <see cref="Matches"/> é a MESMA
/// regra como predicado traduzível para SQL (filtro/contagem). As duas formas moram aqui, lado a
/// lado, para não divergirem — inclusive na completude (branco = ausente, via Trim).
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

        return IsComplete(socialName, contactEmail)
            ? EBrokerageSituation.Active
            : EBrokerageSituation.Incomplete;
    }

    /// <summary>Completude do cadastro (nome fantasia e e-mail de contato presentes).</summary>
    public static bool IsComplete(string? socialName, string? contactEmail)
        => !string.IsNullOrWhiteSpace(socialName) && !string.IsNullOrWhiteSpace(contactEmail);

    /// <summary>
    /// RN-102 — a mesma regra de <see cref="Resolve"/> como predicado sobre a Pessoa, traduzível
    /// para SQL (filtro e contagem por situação, sobre o papel Corretor). A completude usa
    /// <c>Trim()</c> (→ <c>LTRIM(RTRIM())</c>) para casar com o <c>IsNullOrWhiteSpace</c> de
    /// <see cref="IsComplete"/>, então valores só-com-espaço contam como ausentes nos dois lados.
    /// </summary>
    public static Expression<Func<Person, bool>> Matches(EBrokerageSituation situation)
        => situation switch
        {
            EBrokerageSituation.Inactive => person => person.Roles.Any(role =>
                role.Role == EPersonRole.Broker && role.Status == EPersonRoleStatus.Inactive),
            EBrokerageSituation.Active => person => person.Roles.Any(role =>
                role.Role == EPersonRole.Broker && role.Status == EPersonRoleStatus.Active
                && person.SocialName != null && person.SocialName.Trim() != ""
                && role.ContactEmail != null && role.ContactEmail.Trim() != ""),
            _ => person => person.Roles.Any(role =>
                role.Role == EPersonRole.Broker && role.Status == EPersonRoleStatus.Active
                && (person.SocialName == null || person.SocialName.Trim() == ""
                    || role.ContactEmail == null || role.ContactEmail.Trim() == "")),
        };
}
