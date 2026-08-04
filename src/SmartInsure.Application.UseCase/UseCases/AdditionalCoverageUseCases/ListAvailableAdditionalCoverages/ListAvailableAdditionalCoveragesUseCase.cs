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
        // RN-103: a Corretora é a do Escopo ativo do acesso (claim, ADR-065), resolvida pelo servidor —
        // nunca informada pelo cliente. Sem Corretora ativa, a operação é recusada.
        var brokerageId = currentUserAccessor.ActiveBrokerageId
            ?? throw new BusinessRuleException(
                "Nenhuma Corretora ativa no acesso para listar coberturas adicionais.");

        _ = await modalityRepository.GetByIdAsync(request.ModalityId, cancellationToken)
            ?? throw new NotFoundException("Modalidade não encontrada.");

        var available = await importedAdditionalCoverageRepository.ListAvailableForModalityAsync(
            brokerageId, request.ModalityId, cancellationToken);

        var items = available
            .OrderBy(coverage => coverage.Name, StringComparer.CurrentCulture)
            .Select(coverage => new AvailableAdditionalCoverageItemResponse(
                coverage.AdditionalCoverageId, coverage.Name))
            .ToList();

        return new ListAvailableAdditionalCoveragesResponse(items);
    }
}
