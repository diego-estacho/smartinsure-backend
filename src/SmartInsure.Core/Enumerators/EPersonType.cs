namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Tipo da Pessoa (glossário), derivado do documento: CPF (11 dígitos) = F (física),
/// CNPJ (14 dígitos) = J (jurídica). Persistido na Pessoa como string (ADR-031);
/// Natureza Jurídica só existe para a jurídica (RN-015).
/// </summary>
public enum EPersonType
{
    F,
    J,
}
