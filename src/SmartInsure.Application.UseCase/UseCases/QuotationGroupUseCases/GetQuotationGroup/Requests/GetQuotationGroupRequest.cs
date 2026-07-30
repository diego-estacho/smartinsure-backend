namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.GetQuotationGroup.Requests;

/// <summary>Entrada da leitura do Grupo de Cotação por id (reidratação do wizard ao atualizar a página).</summary>
public sealed record GetQuotationGroupRequest(Guid Id);
