namespace SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Requests;

/// <summary>
/// Busca de Pessoa (RN-013): termo é trecho de nome ou CNPJ; o papel do
/// contexto (Insured/PolicyHolder, nomes estáveis do glossário) muda o filtro (RN-016).
/// </summary>
public sealed record SearchPersonsRequest(string Term, string Role);
