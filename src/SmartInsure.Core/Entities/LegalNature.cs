namespace SmartInsure.Core.Entities;

/// <summary>
/// Natureza Jurídica (glossário): código oficial CONCLA que classifica a Pessoa
/// como setor público ou privado (RN-015). Dado de referência mantido por migration —
/// a plataforma só lê.
/// </summary>
public sealed class LegalNature : EntityBase
{
    private LegalNature()
    {
    }

    public int EmissionYear { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public bool IsPrivate { get; private set; }

    public static LegalNature Create(int emissionYear, string code, string name, bool isPrivate)
        => new()
        {
            EmissionYear = emissionYear,
            Code = new string([.. code.Where(char.IsDigit)]),
            Name = name.Trim(),
            IsPrivate = isPrivate,
        };
}
