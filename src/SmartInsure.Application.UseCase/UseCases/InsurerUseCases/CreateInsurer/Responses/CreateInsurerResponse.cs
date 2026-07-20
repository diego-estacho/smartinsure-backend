namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer.Responses;

/// <summary>Dados de saída do cadastro de Seguradora.</summary>
public sealed record CreateInsurerResponse(
    Guid Id,
    string Cnpj,
    string CorporateName,
    string? TradeName,
    string? LogoUrl,
    Guid? ReferenceExternalId,
    string Status);
