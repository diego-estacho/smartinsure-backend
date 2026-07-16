namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.GetInsurer.Requests;

/// <summary>Consulta de detalhe de Seguradora do catálogo (RN-010).</summary>
public sealed record GetInsurerRequest(Guid InsurerId);
