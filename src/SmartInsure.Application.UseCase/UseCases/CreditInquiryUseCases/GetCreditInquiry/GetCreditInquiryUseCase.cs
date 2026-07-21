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

        // Carrega Insurers para montagem dos nomes (batch para evitar N+1).
        var insurerIds = inquiry.Results.Select(r => r.InsurerId).Distinct().ToList();
        var insurers = await insurerRepository.GetCorporateNamesByIdsAsync(insurerIds, cancellationToken);

        var available = inquiry.Results.Where(r => r.Status == ECreditInquiryResultStatus.Available).ToList();

        // RN-029: consolidado = soma do MAIOR AvailableLimit entre grupos POR seguradora disponível.
        var consolidatedLimit = available
            .Sum(r =>
            {
                var maxLimit = r.Limits
                    .Select(l => l.AvailableLimit)
                    .DefaultIfEmpty(0)
                    .Max();
                return maxLimit;
            });

        var resultResponses = inquiry.Results
            .Select(result =>
                new CreditInquiryResultResponse(
                    result.InsurerId,
                    insurers.TryGetValue(result.InsurerId, out var name) ? name : "Seguradora desconhecida",
                    result.Status.ToString(),
                    result.FailureReason,
                    inquiry.PolicyHolderName,
                    result.Limits
                        .Select(l => new CreditInquiryLimitGroupResponse(
                            l.GroupName,
                            l.GroupType,
                            l.AvailableLimit,
                            Math.Max(0, l.RevisedLimit - l.AvailableLimit),
                            l.Rate))
                        .ToList()))
            .ToList();

        return new GetCreditInquiryResponse(
            inquiry.Id,
            inquiry.QueriedAt,
            inquiry.PolicyHolderCnpj,
            inquiry.PolicyHolderName,
            new CreditInquirySummary(
                inquiry.Results.Count,
                available.Count,
                consolidatedLimit),
            resultResponses);
    }
}
