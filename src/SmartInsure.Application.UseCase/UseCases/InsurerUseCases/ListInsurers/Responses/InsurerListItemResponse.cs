namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ListInsurers.Responses;

/// <summary>Item de listagem do catálogo de Seguradoras (RN-008).</summary>
public sealed record InsurerListItemResponse(
    Guid Id,
    string Cnpj,
    string CorporateName,
    string? TradeName,
    string? LogoUrl,
    string Status);
