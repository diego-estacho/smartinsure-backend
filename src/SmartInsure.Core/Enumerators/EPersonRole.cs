namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Papel da Pessoa no contexto de uso (glossário: Segurado, Corretor e Tomador).
/// O papel não é persistido na Pessoa — é o contexto de quem busca/referencia
/// (RN-013/RN-016); a mesma Pessoa pode figurar em papéis diferentes.
/// </summary>
public enum EPersonRole
{
    Insured,
    Broker,
    PolicyHolder,
}
