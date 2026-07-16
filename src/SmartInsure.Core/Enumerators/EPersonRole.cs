namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Papel da Pessoa no contexto da busca (glossário: Segurado e Tomador).
/// O papel não é persistido na pessoa — é o contexto de quem busca (RN-013/RN-016).
/// </summary>
public enum EPersonRole
{
    Insured,
    PolicyHolder,
}
