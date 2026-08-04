using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SubmitQuotationTerms.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SubmitQuotationTerms.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SubmitQuotationTerms.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SubmitQuotationTerms;

/// <summary>
/// RN-063 — "Baixar minuta": envia ao provedor os termos preenchidos da Cotação (UpdateProposalTerms) e,
/// em seguida, obtém a minuta gerada (GetProposalContractDraft). A conexão/motor vêm da Habilitação da
/// Corretora com a Seguradora da Cotação (RN-023). Exige que a Cotação tenha proposta no provedor
/// (ProposalExternalId — só existe em Cotação obtida). Uma falha isolada sobe como erro da requisição —
/// o front não descarta o preenchimento local (CA-07).
/// </summary>
public sealed class SubmitQuotationTermsUseCase(
    IQuotationRepository quotationRepository,
    IPersonRepository personRepository,
    IBrokerageInsurerEnablementRepository enablementRepository,
    ICurrentUserAccessor currentUserAccessor,
    IUnitOfWork unitOfWork,
    IServiceProvider serviceProvider) : ISubmitQuotationTermsUseCase
{
    // camelCase (Web) para a minuta capturada casar com o contrato/leitura do front (RN-062).
    private static readonly JsonSerializerOptions MinutaJsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<SubmitQuotationTermsResponse> ExecuteAsync(
        SubmitQuotationTermsRequest request, CancellationToken cancellationToken)
    {
        // RN-103: a Corretora do envio é a do Escopo ativo do acesso (claim, ADR-065), resolvida pelo
        // servidor — nunca informada pelo cliente. Sem Corretora ativa, a operação é recusada.
        var brokerageId = currentUserAccessor.ActiveBrokerageId
            ?? throw new BusinessRuleException("Nenhuma Corretora ativa no acesso para enviar a minuta.");

        var quotation = await quotationRepository.GetByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new NotFoundException("Cotação não encontrada.");

        // RN-062: a Cotação da rota tem de pertencer ao Grupo da rota (evita submeter minuta de outro Grupo).
        if (quotation.QuotationGroupId != request.QuotationGroupId)
        {
            throw new BusinessRuleException("A Cotação não pertence a este Grupo de Cotação.");
        }

        // RN-063: só há termos a enviar quando existe proposta no provedor (Cotação obtida com id externo).
        if (string.IsNullOrWhiteSpace(quotation.ProposalExternalId))
        {
            throw new BusinessRuleException(
                "A Cotação não possui proposta no provedor — não é possível enviar a minuta.");
        }

        var enablement = await enablementRepository.GetByPairAsync(
                brokerageId, quotation.InsurerId, cancellationToken)
            ?? throw new NotFoundException("Habilitação da Seguradora não encontrada.");

        if (enablement.Status != EBrokerageInsurerEnablementStatus.Active)
        {
            throw new BusinessRuleException("Habilitação da Seguradora está inativa.");
        }

        var brokerage = await personRepository.GetByIdAsync(brokerageId, cancellationToken)
            ?? throw new NotFoundException("Corretora não encontrada.");

        var engine = serviceProvider.GetKeyedService<ICalculationEngine>(enablement.CalculationEngine)
            ?? throw new BusinessRuleException("O motor de cálculo não está disponível na plataforma.");

        var submitInput = new SubmitProposalTermsInput
        {
            BrokerCnpj = brokerage.DocumentNumber,
            ProposalExternalId = quotation.ProposalExternalId!,
            Terms = request.Terms
                .Select(term => new ProposalTermInput(term.Name, term.Value))
                .ToList(),
            ParticularClauses = request.ParticularClauses
                .Select(clause => new ProposalParticularClauseInput(
                    ParseClauseId(clause.ParticularClauseExternalId),
                    clause.Tags.Select(tag => new ProposalTermInput(tag.Name, tag.Value)).ToList()))
                .ToList(),
        };

        // RN-062: captura a minuta preenchida na Cotação selecionada antes de acionar o provedor — o
        // preenchimento fica persistido mesmo quando a geração da minuta falha (CA-07) e sobrevive a um refresh.
        quotation.SetMinuta(
            JsonSerializer.Serialize(request.Terms, MinutaJsonOptions),
            JsonSerializer.Serialize(request.ParticularClauses, MinutaJsonOptions));
        quotationRepository.Update(quotation);
        await unitOfWork.CommitAsync(cancellationToken);

        await engine.SubmitProposalTermsAsync(enablement.ConnectionParameters, submitInput, cancellationToken);

        var draft = await engine.GetProposalContractDraftAsync(
            enablement.ConnectionParameters, brokerage.DocumentNumber, quotation.ProposalExternalId!, cancellationToken);

        return new SubmitQuotationTermsResponse(draft.Url, draft.ExternalId, draft.CreatedAt);
    }

    // As Cláusulas particulares do provedor são identificadas por id inteiro; o catálogo importado guarda
    // esse id como ExternalId (string). Id não-numérico é dado inválido (RN-048).
    private static int ParseClauseId(string externalId)
        => int.TryParse(externalId, out var id)
            ? id
            : throw new BusinessRuleException($"Cláusula particular com identificador inválido: '{externalId}'.");
}
