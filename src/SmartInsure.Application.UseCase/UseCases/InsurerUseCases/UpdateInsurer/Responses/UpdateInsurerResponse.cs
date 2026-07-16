namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Responses;

/// <summary>Dados de saída da alteração cadastral de Seguradora.</summary>
public sealed record UpdateInsurerResponse(
    Guid Id,
    string Cnpj,
    string CorporateName,
    string? TradeName,
    string? LogoUrl,
    string Status);
