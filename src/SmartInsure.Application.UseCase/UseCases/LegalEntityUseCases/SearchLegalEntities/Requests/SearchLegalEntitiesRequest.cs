namespace SmartInsure.Application.UseCase.UseCases.LegalEntityUseCases.SearchLegalEntities.Requests;

/// <summary>
/// Busca de Pessoa Jurídica (RN-013): termo é trecho de nome ou CNPJ; o papel do
/// contexto (Insured/PolicyHolder, nomes estáveis do glossário) muda o filtro (RN-016).
/// </summary>
public sealed record SearchLegalEntitiesRequest(string Term, string Role);
