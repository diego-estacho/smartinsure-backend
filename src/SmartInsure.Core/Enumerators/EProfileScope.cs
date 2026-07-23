namespace SmartInsure.Core.Enumerators;

/// <summary>
/// Escopo do Perfil (glossário `ProfileScope`, RN-032): domínio em que o Perfil vale.
/// Persistido como string (ADR-031).
/// </summary>
public enum EProfileScope
{
    /// <summary>Perfil da plataforma (ex.: Administrador do Sistema); não se vincula a Corretora nem Tomador.</summary>
    System,

    /// <summary>Perfil de uma Corretora específica.</summary>
    Brokerage,

    /// <summary>Perfil de um Tomador específico.</summary>
    PolicyHolder,
}
