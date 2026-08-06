namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ListAvailableAdditionalCoverages.Requests;

/// <summary>
/// RN-104: Coberturas Adicionais ofertáveis na etapa de risco para uma Modalidade. A Corretora é a do
/// Escopo ativo do acesso (RN-103, ADR-065), resolvida pelo servidor — nunca informada pelo cliente.
/// </summary>
public sealed record ListAvailableAdditionalCoveragesRequest(Guid ModalityId);
