using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationMinuta.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationMinuta.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationMinuta.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationMinuta;

/// <summary>
/// RN-062 — Minuta da Cotação selecionada: lê, do catálogo já importado (Tag/Cláusulas da Modalidade
/// Importada da Seguradora), as Tags do objeto e as Cláusulas particulares ativas para o corretor
/// preencher/marcar. Sem catálogo importado para a Seguradora/Modalidade, a minuta vem vazia.
/// </summary>
public sealed class GetQuotationMinutaUseCase(
    IQuotationRepository quotationRepository,
    IQuotationGroupRepository quotationGroupRepository,
    IImportedModalityRepository importedModalityRepository,
    IImportedModalityTagRepository tagRepository,
    IImportedModalityParticularClauseRepository clauseRepository) : IGetQuotationMinutaUseCase
{
    public async Task<QuotationMinutaResponse> ExecuteAsync(
        GetQuotationMinutaRequest request, CancellationToken cancellationToken)
    {
        var quotation = await quotationRepository.GetByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new NotFoundException("Cotação não encontrada.");

        // RN-062: a Cotação da rota tem de pertencer ao Grupo da rota (evita ler minuta de outro Grupo).
        if (quotation.QuotationGroupId != request.QuotationGroupId)
        {
            throw new BusinessRuleException("A Cotação não pertence a este Grupo de Cotação.");
        }

        var group = await quotationGroupRepository.GetByIdAsync(quotation.QuotationGroupId, cancellationToken)
            ?? throw new NotFoundException("Grupo de Cotação não encontrado.");

        var imported = await importedModalityRepository.GetActiveByInsurerAndModalityAsync(
            quotation.InsurerId, group.ModalityId, cancellationToken);

        // Sem catálogo importado para a Seguradora/Modalidade: minuta vazia (não exibe blocos).
        if (imported is null)
        {
            return new QuotationMinutaResponse(null, [], quotation.MinutaTagsJson, quotation.MinutaClausesJson);
        }

        var tag = await tagRepository.GetByImportedModalityAsync(imported.Id, cancellationToken);
        var clauses = await clauseRepository.ListByImportedModalityAsync(imported.Id, cancellationToken);

        var clauseResponses = clauses
            .Where(clause => clause.Status == EImportedModalityClauseStatus.Active)
            .Select(clause => new QuotationMinutaClauseResponse(
                clause.ExternalId, clause.Name, clause.ClauseText, clause.JsonTag))
            .ToList();

        var tagJson = tag?.Status == EImportedModalityTagStatus.Active ? tag.JsonTag : null;

        return new QuotationMinutaResponse(
            tagJson, clauseResponses, quotation.MinutaTagsJson, quotation.MinutaClausesJson);
    }
}
