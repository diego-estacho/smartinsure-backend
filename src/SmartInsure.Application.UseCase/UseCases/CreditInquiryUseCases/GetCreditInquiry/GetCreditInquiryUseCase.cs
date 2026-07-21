using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Responses;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.GetCreditInquiry.Interfaces;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.GetCreditInquiry.Requests;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.GetCreditInquiry.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.GetCreditInquiry;

/// <summary>RN-031: recupera histórico de uma consulta de crédito.</summary>
public sealed class GetCreditInquiryUseCase(
    ICreditInquiryRepository creditInquiryRepository,
    IInsurerRepository insurerRepository) : IGetCreditInquiryUseCase
{
    public async Task<GetCreditInquiryResponse> ExecuteAsync(
        GetCreditInquiryRequest request,
        CancellationToken cancellationToken)
    {
        var inquiry = await creditInquiryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Consulta de crédito não encontrada.");

        // Carrega Insurers para montagem dos nomes.
        var insurerIds = inquiry.Results.Select(r => r.InsurerId).Distinct().ToList();
        var insurers = new Dictionary<Guid, string>();

        foreach (var insurerId in insurerIds)
        {
            var insurer = await insurerRepository.GetByIdAsync(insurerId, cancellationToken);
            if (insurer is not null)
            {
                insurers[insurerId] = insurer.CorporateName;
            }
        }

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
                new CreditInquiryResultResponse(
                    result.InsurerId,
                    insurers.TryGetValue(result.InsurerId, out var name) ? name : "Seguradora desconhecida",
                    result.Status.ToString(),
                    result.FailureReason,
                    result.TraditionalLimit,
                    result.TraditionalRate,
                    result.JudicialLimit,
                    result.JudicialRate,
                    result.JudicialFiscalRate,
                    result.FinancialLimit,
                    result.FinancialRate,
                    result.LimitValidUntil))
            .ToList();

        return new GetCreditInquiryResponse(
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
