using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Aceite do Termo (RN-506): registro do ato de aceitar o Termo da Seguradora. Guarda o **conteúdo
/// exato** exibido, não só um ponteiro para o Termo — se o texto vigente mudar depois, o que vale é o
/// que a pessoa leu. Preservado mesmo que a solicitação de emissão falhe adiante: o aceite aconteceu.
/// </summary>
public sealed class TermAcceptance : EntityBase
{
    private TermAcceptance()
    {
    }

    public Guid InsurerTermId { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>Texto integral que foi exibido e aceito.</summary>
    public string AcceptedContent { get; private set; } = string.Empty;

    /// <summary>Agente de acesso informado (navegador/dispositivo), como veio da borda.</summary>
    public string? UserAgent { get; private set; }

    public DateTime AcceptedAt { get; private set; }

    public static TermAcceptance Register(
        InsurerTerm term, Guid userId, string? userAgent, DateTime acceptedAt)
    {
        if (term is null)
        {
            throw new BusinessRuleException("Não há Termo da Seguradora para aceitar.");
        }

        return new TermAcceptance
        {
            InsurerTermId = term.Id,
            UserId = userId,
            AcceptedContent = term.Content,
            UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim(),
            AcceptedAt = acceptedAt,
        };
    }
}
