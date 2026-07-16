namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Requests;

/// <summary>Dados de entrada para alterar cadastro de uma Seguradora (RN-006).</summary>
/// <param name="InsurerId">Identificador único da seguradora.</param>
/// <param name="Cnpj">CNPJ da seguradora (com ou sem máscara).</param>
/// <param name="CorporateName">Razão social.</param>
/// <param name="TradeName">Nome fantasia (opcional).</param>
/// <param name="LogoUrl">Endereço externo do logotipo (opcional).</param>
public sealed record UpdateInsurerRequest(
    Guid InsurerId,
    string Cnpj,
    string CorporateName,
    string? TradeName,
    string? LogoUrl);
