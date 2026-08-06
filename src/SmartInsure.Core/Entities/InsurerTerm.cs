using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Termo da Seguradora (RN-506): texto do Termo e declaração que a Seguradora exige que o corretor
/// aceite para emitir. Há uma versão vigente por Seguradora; substituir o texto cria uma versão nova e
/// inativa a anterior, porque aceite já registrado aponta para o texto que foi de fato exibido.
/// Sem Termo vigente a Seguradora não é emitível pela plataforma.
/// </summary>
public sealed class InsurerTerm : EntityBase
{
    private InsurerTerm()
    {
    }

    public Guid InsurerId { get; private set; }

    /// <summary>Texto integral apresentado ao corretor.</summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>Versão vigente da Seguradora; apenas uma por Seguradora.</summary>
    public bool IsActive { get; private set; }

    public static InsurerTerm Create(Guid insurerId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new BusinessRuleException("O Termo da Seguradora não pode ser vazio.");
        }

        return new InsurerTerm
        {
            InsurerId = insurerId,
            Content = content.Trim(),
            IsActive = true,
        };
    }

    /// <summary>Sai de vigência quando uma versão nova assume; o texto é preservado para os aceites.</summary>
    public void Deactivate() => IsActive = false;
}
