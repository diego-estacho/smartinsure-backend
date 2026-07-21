using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Interfaces;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Requests;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Responses;
using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry;

/// <summary>
/// RN-029..031 — Consulta de Limites de Crédito do Tomador: dispara consulta paralela ao motor
/// de cada Habilitação Ativa da Corretora; tolerando falha isolada por seguradora (RN-030);
/// grava histórico imutável com data/hora e resultados.
/// </summary>
public sealed class ExecuteCreditInquiryUseCase(
    IBrokerageInsurerEnablementRepository enablementRepository,
    IInsurerRepository insurerRepository,
    IPersonRepository personRepository,
    ICreditInquiryRepository creditInquiryRepository,
    IUnitOfWork unitOfWork,
    IServiceProvider serviceProvider) : IExecuteCreditInquiryUseCase
{
    public async Task<ExecuteCreditInquiryResponse> ExecuteAsync(
        ExecuteCreditInquiryRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedCnpj = CnpjValidator.Normalize(request.PolicyHolderCnpj);

        // RN-029: carrega Corretora (Person papel Broker).
        var brokerage = await personRepository.GetBrokerageByIdAsync(request.BrokerageId, cancellationToken)
            ?? throw new NotFoundException("Corretora não encontrada.");

        // RN-029: lista Habilitações Ativas da Corretora.
        var activeEnablements = await enablementRepository.ListActiveByBrokerageAsync(
            request.BrokerageId, cancellationToken);

        if (activeEnablements.Count == 0)
        {
            throw new BusinessRuleException(
                "A corretora não possui seguradoras habilitadas para consulta de crédito.");
        }

        // Carregamento de dados ANTES do fan-out (DbContext não é thread-safe).
        // Para cada habilitação: carrega Insurer, pega motor e parâmetros.
        var insurerData = new Dictionary<Guid, (Insurer insurer, ICalculationEngine engine, string connectionParams)>();

        foreach (var enablement in activeEnablements)
        {
            var insurer = await insurerRepository.GetByIdAsync(enablement.InsurerId, cancellationToken)
                ?? throw new NotFoundException($"Seguradora {enablement.InsurerId} não encontrada.");

            var engine = ResolveEngine(enablement.CalculationEngine);

            insurerData[enablement.InsurerId] = (insurer, engine, enablement.ConnectionParameters ?? string.Empty);
        }

        // RN-030: fan-out de chamadas ao motor (apenas HTTP, sem EF) com tolerância a falha isolada.
        var creditInquiry = CreditInquiry.Create(request.BrokerageId, normalizedCnpj);

        var motorTasks = activeEnablements
            .Select(enablement => ExecuteMotorCallAsync(
                creditInquiry.Id,
                enablement.InsurerId,
                insurerData[enablement.InsurerId],
                brokerage.DocumentNumber,
                normalizedCnpj,
                cancellationToken))
            .ToList();

        await Task.WhenAll(motorTasks);

        // Coleta resultados (sempre sucesso, mesmo com falhas isoladas — RN-030).
        foreach (var task in motorTasks)
        {
            var result = await task;
            creditInquiry.AddResult(result);
        }

        // RN-031: persiste histórico imutável com todos os resultados.
        await creditInquiryRepository.AddAsync(creditInquiry, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        // Constrói response com resumo consolidado (RN-030: apenas Available).
        return BuildResponse(creditInquiry, insurerData);
    }

    private async Task<CreditInquiryResult> ExecuteMotorCallAsync(
        Guid creditInquiryId,
        Guid insurerId,
        (Insurer insurer, ICalculationEngine engine, string connectionParams) data,
        string brokerageCnpj,
        string policyHolderCnpj,
        CancellationToken cancellationToken)
    {
        var (insurer, engine, connectionParams) = data;

        // RN-010/RN-023: Insurer inativa → Unavailable.
        if (insurer.Status != EInsurerStatus.Active)
        {
            return CreditInquiryResult.Unavailable(
                creditInquiryId, insurerId, "Seguradora está inativa no catálogo.");
        }

        // RN-023: sem ReferenceExternalId → Unavailable.
        if (string.IsNullOrWhiteSpace(insurer.ReferenceExternalId))
        {
            return CreditInquiryResult.Unavailable(
                creditInquiryId, insurerId, "Identificador externo da seguradora não configurado.");
        }

        try
        {
            // RN-029: consulta limites de crédito via motor configurado na habilitação.
            var limits = await engine.GetPolicyHolderLimitsAndRatesAsync(
                connectionParams,
                brokerageCnpj,
                policyHolderCnpj,
                insurer.ReferenceExternalId,
                cancellationToken);

            // RN-030: resposta nula (indisponibilidade no motor) → Unavailable.
            if (limits is null)
            {
                return CreditInquiryResult.Unavailable(
                    creditInquiryId, insurerId, "Seguradora indisponível ou tomador sem limite de crédito.");
            }

            return CreditInquiryResult.Available(
                creditInquiryId,
                insurerId,
                limits.TraditionalLimit,
                limits.TraditionalRate,
                limits.JudicialLimit,
                limits.JudicialRate,
                limits.JudicialFiscalRate,
                limits.FinancialLimit,
                limits.FinancialRate,
                limits.LimitValidUntil);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (CalculationEngineException exception)
        {
            // RN-030: exceção de integração (não é negócio) → Unavailable com motivo.
            return CreditInquiryResult.Unavailable(
                creditInquiryId, insurerId, $"Falha na integração: {exception.Message}");
        }
    }

    private ICalculationEngine ResolveEngine(ECalculationEngine engineType)
    {
        return serviceProvider.GetKeyedService<ICalculationEngine>(engineType)
            ?? throw new BusinessRuleException("O motor de cálculo não está disponível na plataforma.");
    }

    private ExecuteCreditInquiryResponse BuildResponse(
        CreditInquiry inquiry,
        Dictionary<Guid, (Insurer insurer, ICalculationEngine engine, string connectionParams)> insurerData)
    {
        var available = inquiry.Results.Where(r => r.Status == ECreditInquiryResultStatus.Available).ToList();

        // Consolidado = soma do MAIOR limite entre modalidades POR seguradora disponível.
        var consolidatedLimit = available
            .Sum(r =>
            {
                var max = new[] { r.TraditionalLimit, r.JudicialLimit, r.FinancialLimit }
                    .Where(l => l.HasValue)
                    .DefaultIfEmpty(0)
                    .Max();
                return max ?? 0;
            });

        var resultResponses = inquiry.Results
            .Select(result =>
            {
                var insurerName = insurerData.TryGetValue(result.InsurerId, out var data)
                    ? data.insurer.CorporateName
                    : "Seguradora desconhecida";

                return new CreditInquiryResultResponse(
                    result.InsurerId,
                    insurerName,
                    result.Status.ToString(),
                    result.FailureReason,
                    result.TraditionalLimit,
                    result.TraditionalRate,
                    result.JudicialLimit,
                    result.JudicialRate,
                    result.JudicialFiscalRate,
                    result.FinancialLimit,
                    result.FinancialRate,
                    result.LimitValidUntil);
            })
            .ToList();

        return new ExecuteCreditInquiryResponse(
            inquiry.Id,
            inquiry.QueriedAt,
            inquiry.PolicyHolderCnpj,
            new CreditInquirySummary(
                inquiry.Results.Count,
                available.Count,
                consolidatedLimit),
            resultResponses);
    }
}
