using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ListAvailableAdditionalCoverages.Interfaces;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ListAvailableAdditionalCoverages.Requests;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ListAvailableAdditionalCoverages.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ListAvailableAdditionalCoverages;

/// <summary>
/// RN-104/RN-046 — Coberturas Adicionais canônicas ofertáveis na etapa de risco para uma Modalidade:
/// união simples das Seguradoras habilitadas da Corretora do Escopo ativo. A disponibilidade é
/// derivada dos vínculos ativos, nunca digitada; o mesmo critério vale para o envio (RN-105), para
/// que oferta e envio não divirjam.
/// </summary>
public sealed class ListAvailableAdditionalCoveragesUseCase(
    IImportedAdditionalCoverageRepository importedAdditionalCoverageRepository,
    IModalityRepository modalityRepository,
    ICurrentUserAccessor currentUserAccessor) : IListAvailableAdditionalCoveragesUseCase
{
    public async Task<ListAvailableAdditionalCoveragesResponse> ExecuteAsync(
        ListAvailableAdditionalCoveragesRequest request,
        CancellationToken cancellationToken)
    {
        _ = await modalityRepository.GetByIdAsync(request.ModalityId, cancellationToken)
            ?? throw new NotFoundException("Modalidade não encontrada.");

        // RN-103: a Corretora é a do Escopo ativo do acesso (claim, ADR-065), resolvida pelo servidor —
        // nunca informada pelo cliente. Escopo ausente é estado legítimo (ADR-065), não violação de
        // regra: a oferta é DERIVADA das Seguradoras habilitadas da Corretora ativa, então sem
        // Corretora ativa o conjunto derivado é simplesmente vazio (RN-104). Recusar aqui quebraria a
        // renderização da etapa de risco; quem recusa por falta de Escopo é o cotar (RN-103), que é
        // ação e devolve mensagem própria.
        var brokerageId = currentUserAccessor.ActiveBrokerageId;

        var available = brokerageId is null
            ? []
            : await importedAdditionalCoverageRepository.ListAvailableForModalityAsync(
                brokerageId.Value, request.ModalityId, cancellationToken);

        var items = available
            .OrderBy(coverage => coverage.Name, StringComparer.CurrentCulture)
            .Select(coverage => new AvailableAdditionalCoverageItemResponse(
                coverage.AdditionalCoverageId, coverage.Name))
            .ToList();

        return new ListAvailableAdditionalCoveragesResponse(items);
    }
}
