namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Requests;

/// <summary>Dados de entrada para alterar cadastro de uma Seguradora (RN-008).</summary>
/// <param name="InsurerId">Identificador único da seguradora.</param>
/// <param name="Cnpj">CNPJ da seguradora (com ou sem máscara).</param>
/// <param name="CorporateName">Razão social.</param>
/// <param name="TradeName">Nome fantasia (opcional).</param>
/// <param name="LogoUrl">Endereço externo do logotipo (opcional).</param>
/// <param name="ReferenceExternalId">Identificador da seguradora no sistema de origem do motor de cálculo (opcional).</param>
public sealed record UpdateInsurerRequest(
    Guid InsurerId,
    string Cnpj,
    string CorporateName,
    string? TradeName,
    string? LogoUrl,
    Guid? ReferenceExternalId = null);
