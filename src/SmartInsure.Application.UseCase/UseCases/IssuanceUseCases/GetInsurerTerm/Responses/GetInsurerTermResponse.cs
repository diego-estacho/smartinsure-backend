namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.GetInsurerTerm.Responses;

/// <summary>
/// RN-506: texto do Termo e declaração a exibir antes de emitir. É o MESMO conteúdo que o aceite
/// registra — o cliente não guarda texto próprio, para não haver divergência entre exibido e assinado.
/// </summary>
public sealed record GetInsurerTermResponse(Guid InsurerId, string Content);
