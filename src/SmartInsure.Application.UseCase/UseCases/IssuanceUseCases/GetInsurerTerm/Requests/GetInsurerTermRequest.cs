namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.GetInsurerTerm.Requests;

/// <summary>RN-506: Termo vigente da Seguradora da Cotação escolhida do Grupo.</summary>
public sealed record GetInsurerTermRequest(Guid QuotationGroupId);
