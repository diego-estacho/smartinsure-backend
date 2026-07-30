namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Situação apresentada da Corretora (RN-102): derivada no servidor a partir do status do
/// papel Corretor (Ativo/Inativo) e da completude do cadastro. Não é status armazenado nem
/// cria transição na máquina de estados (glossário — status segue Active/Inactive).
/// </summary>
public enum EBrokerageSituation
{
    Active,
    Incomplete,
    Inactive,
}
