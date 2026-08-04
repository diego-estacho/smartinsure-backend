namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance.Responses;

/// <summary>RN-506: identificação do aceite registrado, referenciada pela Apólice (RN-514).</summary>
public sealed record RegisterTermAcceptanceResponse
{
    public required Guid TermAcceptanceId { get; init; }

    public required DateTime AcceptedAt { get; init; }
}
