namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Tipo da Pessoa (glossário), derivado do documento: CPF (11 dígitos) = física,
/// CNPJ (14 dígitos) = jurídica. Persistido na Pessoa; Natureza Jurídica só existe
/// para a jurídica (RN-015).
/// </summary>
public enum EPersonType
{
    Natural,
    Legal,
}
