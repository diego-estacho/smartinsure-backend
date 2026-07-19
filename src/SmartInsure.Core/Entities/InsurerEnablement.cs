using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Habilitação de Seguradora (RN-022): vínculo único entre a Corretora (Pessoa com papel
/// Corretor) e a Seguradora, com o Motor de Cálculo e os parâmetros de conexão do par.
/// Nunca excluída — Inativa suspende as operações do par (RN-023).
/// </summary>
public sealed class InsurerEnablement : EntityBase
{
    private InsurerEnablement()
    {
    }

    public Guid BrokerageId { get; private set; }

    public Guid InsurerId { get; private set; }

    public ECalculationEngine CalculationEngine { get; private set; }

    public string? ConnectionParameters { get; private set; }

    public EInsurerEnablementStatus Status { get; private set; }

    /// <summary>RN-022: a Habilitação nasce Ativa.</summary>
    public static InsurerEnablement Create(
        Guid brokerageId,
        Guid insurerId,
        ECalculationEngine calculationEngine,
        string? connectionParameters)
        => new()
        {
            BrokerageId = brokerageId,
            InsurerId = insurerId,
            CalculationEngine = calculationEngine,
            ConnectionParameters = Normalize(connectionParameters),
            Status = EInsurerEnablementStatus.Active,
        };

    /// <summary>RN-022: alteração do motor e dos parâmetros de conexão; o par e a situação não mudam aqui.</summary>
    public void UpdateSettings(ECalculationEngine calculationEngine, string? connectionParameters)
    {
        CalculationEngine = calculationEngine;
        ConnectionParameters = Normalize(connectionParameters);
    }

    /// <summary>RN-022: Inativa → Ativa; ativar quem já está Ativa é conflito de estado.</summary>
    public void Activate()
    {
        if (Status == EInsurerEnablementStatus.Active)
        {
            throw new ConflictException("A habilitação já está ativa.");
        }

        Status = EInsurerEnablementStatus.Active;
    }

    /// <summary>RN-022: Ativa → Inativa (suspensa, nunca excluída).</summary>
    public void Deactivate()
    {
        if (Status == EInsurerEnablementStatus.Inactive)
        {
            throw new ConflictException("A habilitação já está inativa.");
        }

        Status = EInsurerEnablementStatus.Inactive;
    }

    private static string? Normalize(string? connectionParameters)
        => string.IsNullOrWhiteSpace(connectionParameters) ? null : connectionParameters.Trim();
}
