using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance.Interfaces;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance.Requests;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RequestPolicyIssuance.Interfaces;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RequestPolicyIssuance.Requests;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RequestPolicyIssuance.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RequestPolicyIssuance;

/// <summary>
/// RN-500 — solicita a emissão da Apólice da Cotação escolhida.
///
/// Primeiro o **portão**: tudo que a plataforma já sabe reprovar é reprovado ANTES de acionar a
/// Seguradora, com motivo específico — Cotação não emitível (RN-500), Contragarantia exigida sem
/// assinatura (RN-501), minuta incompleta (RN-502), endereço do Segurado ausente (RN-503), pagamento
/// fora das opções informadas (RN-505), Termo não aceito (RN-506) e emissão já solicitada (RN-507).
/// Chamada mutante não é gasta em caminho previsível de recusa, e o aceite do Termo não é queimado.
///
/// Depois a **sequência** (síncrona, um commit — decisão do dono): reenvia os termos vigentes (RN-502),
/// registra e comunica o aceite (RN-506), solicita a emissão e, no sucesso, registra a Apólice e promove
/// a oferta a Emissão solicitada (RN-508/RN-514). Recusa da Seguradora não registra Apólice e mantém a
/// oferta Cotada (RN-511). A Seguradora é acionada pela Habilitação que obteve a Cotação (RN-512).
/// </summary>
public sealed class RequestPolicyIssuanceUseCase(
    IQuotationGroupRepository quotationGroupRepository,
    IQuotationRepository quotationRepository,
    IPolicyRepository policyRepository,
    IBrokerageInsurerEnablementRepository enablementRepository,
    IPersonRepository personRepository,
    IInsurerRepository insurerRepository,
    IRegisterTermAcceptanceUseCase registerTermAcceptanceUseCase,
    IUserRepository userRepository,
    ICurrentUserAccessor currentUserAccessor,
    IUnitOfWork unitOfWork,
    IServiceProvider serviceProvider) : IRequestPolicyIssuanceUseCase
{
    private static readonly JsonSerializerOptions MinutaJsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<RequestPolicyIssuanceResponse> ExecuteAsync(
        RequestPolicyIssuanceRequest request,
        CancellationToken cancellationToken)
    {
        var group = await quotationGroupRepository.GetByIdAsync(request.QuotationGroupId, cancellationToken)
            ?? throw new NotFoundException("Grupo de cotação não encontrado.");

        var quotation = await LoadSelectedQuotationAsync(group, cancellationToken);

        await EnsureIssuableAsync(group, quotation, request, cancellationToken);

        var user = await ResolveCurrentUserAsync(cancellationToken);
        var enablement = await ResolveEnablementAsync(quotation, cancellationToken);
        var engine = ResolveEngine(enablement);
        var brokerCnpj = await ResolveBrokerCnpjAsync(group, cancellationToken);
        var insurer = await insurerRepository.GetByIdAsync(quotation.InsurerId, cancellationToken)
            ?? throw new BusinessRuleException("Seguradora da cotação escolhida não encontrada.");

        if (string.IsNullOrWhiteSpace(insurer.ReferenceExternalId))
        {
            throw new BusinessRuleException("Identificador externo da Seguradora não configurado.");
        }

        // RN-506: o aceite é fato consumado e fica registrado; a Seguradora é avisada em seguida.
        var acceptance = await registerTermAcceptanceUseCase.ExecuteAsync(
            new RegisterTermAcceptanceRequest { InsurerId = quotation.InsurerId, UserAgent = request.UserAgent },
            cancellationToken);

        PolicyIssuanceResult issuance;

        try
        {
            // RN-502: os termos que valem são os vigentes na Cotação — reenviados imediatamente antes do
            // pedido, para a Apólice sair com o que o corretor está vendo.
            await engine.SubmitProposalTermsAsync(
                enablement.ConnectionParameters, BuildTermsInput(quotation, brokerCnpj), cancellationToken);

            await engine.SubmitPolicyAcceptanceTermAsync(
                enablement.ConnectionParameters, brokerCnpj, quotation.ProposalExternalId!, cancellationToken);

            issuance = await engine.CreatePolicyAsync(
                enablement.ConnectionParameters,
                new CreatePolicyInput
                {
                    BrokerCnpj = brokerCnpj,
                    ProposalExternalId = quotation.ProposalExternalId!,
                    InsuranceUniqueId = insurer.ReferenceExternalId,
                    InstallmentNumber = request.InstallmentNumber,
                    GracePeriodInDays = request.GracePeriodInDays,
                    InsuredAddress = new IssuanceAddressInput
                    {
                        ZipCode = group.InsuredAddress!.ZipCode,
                        Street = group.InsuredAddress.Street,
                        Number = group.InsuredAddress.Number,
                        Complement = group.InsuredAddress.Complement,
                        Neighborhood = group.InsuredAddress.Neighborhood,
                        City = group.InsuredAddress.City,
                        State = group.InsuredAddress.State,
                    },
                },
                cancellationToken);
        }
        catch (CalculationEngineException exception)
        {
            // RN-511: veredito é da Seguradora — nenhuma Apólice registrada, oferta segue Cotada, motivo dela.
            throw new BusinessRuleException(exception.Message);
        }

        var policy = Policy.RequestIssuance(
            quotation,
            group,
            issuance.PolicyExternalId,
            issuance.ProposalNumber ?? quotation.ProposalNumber,
            request.InstallmentNumber,
            request.GracePeriodInDays,
            acceptance.TermAcceptanceId,
            user.Id,
            DateTime.UtcNow);

        await policyRepository.AddAsync(policy, cancellationToken);

        group.MarkEmissionRequested();
        quotationGroupRepository.Update(group);

        await unitOfWork.CommitAsync(cancellationToken);

        // RN-509: emitida uma, as irmãs não têm por que seguir abertas na Seguradora — proposta aberta
        // tende a reter Limite de Crédito do Tomador. Falhar aqui NÃO desfaz a emissão já registrada.
        await CancelSiblingQuotationsAsync(group, quotation, brokerCnpj, cancellationToken);

        return new RequestPolicyIssuanceResponse
        {
            PolicyId = policy.Id,
            PolicyExternalId = policy.PolicyExternalId,
            ProposalNumber = policy.ProposalNumber,
            RequestedAt = policy.RequestedAt,
            QuotationGroupStatus = group.Status.ToString(),
        };
    }

    /// <summary>
    /// RN-509: cancela, junto às respectivas Seguradoras, as demais Cotações do Grupo que estejam em
    /// condição de cancelamento — as indisponíveis, recusadas e não reconhecidas não são tocadas, e
    /// tampouco Cotações de outros Grupos do mesmo fork (RN-060). O insucesso de uma irmã não interrompe
    /// as outras nem invalida a emissão: o corretor não é bloqueado por isso.
    /// </summary>
    private async Task CancelSiblingQuotationsAsync(
        QuotationGroup group,
        Quotation issued,
        string brokerCnpj,
        CancellationToken cancellationToken)
    {
        var quotations = await quotationRepository.ListByGroupAsync(group.Id, cancellationToken);

        foreach (var sibling in quotations)
        {
            if (sibling.Id == issued.Id
                || sibling.Result != EQuotationResult.ReadyForEmission
                || string.IsNullOrWhiteSpace(sibling.ProposalExternalId)
                || sibling.BrokerageInsurerEnablementId is null)
            {
                continue;
            }

            try
            {
                var siblingEnablement = await enablementRepository.GetByIdAsync(
                    sibling.BrokerageInsurerEnablementId.Value, cancellationToken);

                if (siblingEnablement is null)
                {
                    continue;
                }

                var siblingEngine = ResolveEngine(siblingEnablement);

                await siblingEngine.CancelProposalAsync(
                    siblingEnablement.ConnectionParameters,
                    new CancelProposalInput
                    {
                        BrokerCnpj = brokerCnpj,
                        ProposalExternalId = sibling.ProposalExternalId,
                        Reason = "Outra cotação desta oferta foi emitida.",
                    },
                    cancellationToken);
            }
            catch (CalculationEngineException)
            {
                // Cancelar irmã é efeito posterior: registrado como insucesso e seguimos (RN-509).
            }
            catch (BusinessRuleException)
            {
                // Motor indisponível para a irmã não invalida a emissão já solicitada.
            }
        }
    }

    private async Task<Quotation> LoadSelectedQuotationAsync(
        QuotationGroup group, CancellationToken cancellationToken)
    {
        if (group.SelectedQuotationId is null)
        {
            throw new BusinessRuleException("Nenhuma cotação foi escolhida nesta oferta.");
        }

        return await quotationRepository.GetByIdAsync(group.SelectedQuotationId.Value, cancellationToken)
            ?? throw new NotFoundException("Cotação escolhida não encontrada.");
    }

    /// <summary>Portão do emitir: cada reprovação com motivo próprio, nenhuma chamada à Seguradora.</summary>
    private async Task EnsureIssuableAsync(
        QuotationGroup group,
        Quotation quotation,
        RequestPolicyIssuanceRequest request,
        CancellationToken cancellationToken)
    {
        if (group.Status == EQuotationGroupStatus.EmissionRequested)
        {
            throw new BusinessRuleException("A emissão desta oferta já foi solicitada.");
        }

        if (await policyRepository.ExistsForQuotationAsync(quotation.Id, cancellationToken))
        {
            throw new BusinessRuleException("A emissão desta cotação já foi solicitada.");
        }

        if (quotation.Result == EQuotationResult.Analysis)
        {
            throw new BusinessRuleException(
                "Esta cotação está em análise na seguradora — o acompanhamento da análise não faz parte desta etapa.");
        }

        if (quotation.Result != EQuotationResult.ReadyForEmission)
        {
            throw new BusinessRuleException("Só uma cotação pronta para emissão pode ser emitida.");
        }

        if (string.IsNullOrWhiteSpace(quotation.ProposalExternalId))
        {
            throw new BusinessRuleException("A cotação escolhida não tem proposta na seguradora.");
        }

        // RN-501: a exigência de Contragarantia é capturada na cotação e enforçada só aqui.
        if (quotation.RequiresCcg && !quotation.CcgSigned)
        {
            throw new BusinessRuleException(
                "A seguradora exige Contragarantia (CCG) assinada para emitir esta apólice.");
        }

        if (!HasFilledMinuta(quotation))
        {
            throw new BusinessRuleException(
                "A minuta precisa estar completa para emitir: preencha todas as tags antes de continuar.");
        }

        if (!group.HasInsuredAddressForIssuance())
        {
            throw new BusinessRuleException(
                "A oferta não tem endereço do segurado completo — corrija o cadastro do segurado e selecione o endereço.");
        }

        if (!request.TermAccepted)
        {
            throw new BusinessRuleException("É obrigatório aceitar o Termo e declaração para emitir.");
        }

        // RN-505: a escolha tem de constar entre as opções que a seguradora informou nesta Cotação.
        if (!quotation.HasInstallmentOption(request.InstallmentNumber))
        {
            throw new BusinessRuleException(
                "O parcelamento escolhido não está entre as opções informadas pela seguradora.");
        }

        if (!quotation.HasGracePeriodOption(request.GracePeriodInDays))
        {
            throw new BusinessRuleException(
                "O vencimento da primeira parcela não está entre as opções informadas pela seguradora.");
        }
    }

    /// <summary>
    /// RN-502: minuta completa é toda Tag com valor. Cotação sem Tag alguma não tem o que preencher —
    /// mas Tag registrada e vazia bloqueia, porque a apólice sairia com lacuna no objeto.
    /// </summary>
    private static bool HasFilledMinuta(Quotation quotation)
    {
        if (string.IsNullOrWhiteSpace(quotation.MinutaTagsJson))
        {
            return false;
        }

        var tags = JsonSerializer.Deserialize<List<MinutaTag>>(quotation.MinutaTagsJson, MinutaJsonOptions) ?? [];

        return tags.Count > 0 && tags.TrueForAll(tag => !string.IsNullOrWhiteSpace(tag.Value));
    }

    private async Task<User> ResolveCurrentUserAsync(CancellationToken cancellationToken)
    {
        var externalIdentity = currentUserAccessor.UserIdentifier;

        if (string.IsNullOrWhiteSpace(externalIdentity))
        {
            throw new BusinessRuleException("A emissão exige um Usuário autenticado.");
        }

        return await userRepository.GetByExternalIdentityAsync(externalIdentity, cancellationToken)
            ?? throw new BusinessRuleException("Usuário autenticado não encontrado na plataforma.");
    }

    /// <summary>RN-512: a Habilitação usada é a que obteve a Cotação — nunca resolvida de novo.</summary>
    private async Task<BrokerageInsurerEnablement> ResolveEnablementAsync(
        Quotation quotation, CancellationToken cancellationToken)
    {
        if (quotation.BrokerageInsurerEnablementId is null)
        {
            throw new BusinessRuleException(
                "A cotação escolhida não registra a habilitação usada para obtê-la.");
        }

        return await enablementRepository.GetByIdAsync(
                quotation.BrokerageInsurerEnablementId.Value, cancellationToken)
            ?? throw new BusinessRuleException("A habilitação da seguradora não está mais disponível.");
    }

    private ICalculationEngine ResolveEngine(BrokerageInsurerEnablement enablement)
        => serviceProvider.GetKeyedService<ICalculationEngine>(enablement.CalculationEngine)
           ?? throw new BusinessRuleException("O motor de cálculo não está disponível na plataforma.");

    private async Task<string> ResolveBrokerCnpjAsync(QuotationGroup group, CancellationToken cancellationToken)
    {
        if (group.BrokerageId is null)
        {
            throw new BusinessRuleException("A oferta não registra a corretora que a cotou.");
        }

        var brokerage = await personRepository.GetByIdAsync(group.BrokerageId.Value, cancellationToken)
            ?? throw new BusinessRuleException("Corretora da oferta não encontrada.");

        return brokerage.DocumentNumber;
    }

    private static SubmitProposalTermsInput BuildTermsInput(Quotation quotation, string brokerCnpj)
    {
        var tags = JsonSerializer.Deserialize<List<MinutaTag>>(
            quotation.MinutaTagsJson ?? "[]", MinutaJsonOptions) ?? [];

        var clauses = JsonSerializer.Deserialize<List<MinutaClause>>(
            quotation.MinutaClausesJson ?? "[]", MinutaJsonOptions) ?? [];

        return new SubmitProposalTermsInput
        {
            BrokerCnpj = brokerCnpj,
            ProposalExternalId = quotation.ProposalExternalId!,
            Terms = tags.Select(tag => new ProposalTermInput(tag.Name, tag.Value)).ToList(),
            ParticularClauses = clauses
                .Select(clause => new ProposalParticularClauseInput(
                    ParseClauseId(clause.ParticularClauseExternalId),
                    (clause.Tags ?? []).Select(tag => new ProposalTermInput(tag.Name, tag.Value)).ToList()))
                .ToList(),
        };
    }

    private static int ParseClauseId(string? externalId)
        => int.TryParse(externalId, out var id)
            ? id
            : throw new BusinessRuleException($"Cláusula particular com identificador inválido: '{externalId}'.");

    /// <summary>Formato em que a minuta foi capturada na Cotação (RN-062).</summary>
    private sealed record MinutaTag(string Name, string Value);

    private sealed record MinutaClause(string? ParticularClauseExternalId, List<MinutaTag>? Tags);
}
