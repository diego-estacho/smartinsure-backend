namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance.Requests;

/// <summary>RN-506: aceite do Termo vigente da Seguradora da Cotação escolhida.</summary>
public sealed record RegisterTermAcceptanceRequest
{
    public required Guid InsurerId { get; init; }

    /// <summary>Agente de acesso (navegador/dispositivo) informado pela borda; parte da prova do aceite.</summary>
    public string? UserAgent { get; init; }
}
