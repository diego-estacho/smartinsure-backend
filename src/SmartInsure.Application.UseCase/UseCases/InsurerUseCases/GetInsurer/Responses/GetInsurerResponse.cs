namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.GetInsurer.Responses;

/// <summary>Dados de detalhe de Seguradora do catálogo (RN-008).</summary>
public sealed record GetInsurerResponse(
    Guid Id,
    string Cnpj,
    string CorporateName,
    string? TradeName,
    string? LogoUrl,
    string Status);
