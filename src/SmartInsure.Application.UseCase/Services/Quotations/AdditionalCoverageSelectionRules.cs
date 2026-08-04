using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.Services.Quotations;

/// <summary>
/// RN-104: validação da escolha de Coberturas Adicionais num Grupo de Cotação. É regra de negócio, do
/// servidor — o cliente limpar a seleção ao trocar a Modalidade é conveniência de UI, não garantia:
/// uma chamada direta ao contrato poderia gravar cobertura que a Modalidade escolhida não oferece, e
/// toda Cotação nasceria com ela "não contemplada" como se fosse limitação das Seguradoras.
/// </summary>
internal static class AdditionalCoverageSelectionRules
{
    /// <summary>
    /// Recusa a escolha quando alguma Cobertura Adicional não está ofertável para a Modalidade nas
    /// Seguradoras habilitadas da Corretora do Escopo ativo (RN-103/RN-104). Usa exatamente a consulta
    /// que alimenta a oferta da etapa de risco, então oferta e gravação não podem divergir.
    /// Lista vazia é escolha válida (nenhuma cobertura) e não exige Corretora ativa.
    /// </summary>
    public static async Task EnsureAvailableForModalityAsync(
        IImportedAdditionalCoverageRepository importedAdditionalCoverageRepository,
        ICurrentUserAccessor currentUserAccessor,
        Guid modalityId,
        IReadOnlyList<Guid>? additionalCoverageIds,
        CancellationToken cancellationToken)
    {
        var chosen = (additionalCoverageIds ?? []).Distinct().ToList();

        if (chosen.Count == 0)
        {
            return;
        }

        var brokerageId = currentUserAccessor.ActiveBrokerageId
            ?? throw new BusinessRuleException(
                "Nenhuma Corretora ativa no acesso para escolher coberturas adicionais.");

        var available = await importedAdditionalCoverageRepository.ListAvailableForModalityAsync(
            brokerageId, modalityId, cancellationToken);

        var availableIds = available.Select(coverage => coverage.AdditionalCoverageId).ToHashSet();

        if (chosen.Any(coverageId => !availableIds.Contains(coverageId)))
        {
            throw new BusinessRuleException(
                "Cobertura adicional indisponível para a modalidade escolhida.");
        }
    }
}
