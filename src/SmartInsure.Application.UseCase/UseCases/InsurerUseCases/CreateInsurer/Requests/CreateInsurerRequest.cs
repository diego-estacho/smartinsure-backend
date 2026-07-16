namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer.Requests;

/// <summary>Dados de entrada para cadastrar uma Seguradora no catálogo (RN-005).</summary>
/// <param name="Cnpj">CNPJ da seguradora (com ou sem máscara).</param>
/// <param name="CorporateName">Razão social.</param>
/// <param name="TradeName">Nome fantasia (opcional).</param>
/// <param name="LogoUrl">Endereço externo do logotipo (opcional).</param>
/// <param name="InitialStatus">Situação inicial pelo nome estável: Active ou Inactive.</param>
public sealed record CreateInsurerRequest(
    string Cnpj,
    string CorporateName,
    string? TradeName,
    string? LogoUrl,
    string InitialStatus);
