using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ListCreditInquiries.Interfaces;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ListCreditInquiries.Requests;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ListCreditInquiries.Responses;
using SmartInsure.Core.Abstractions.Repositories;

namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ListCreditInquiries;

/// <summary>RN-031: lista histórico paginado de consultas de crédito com filtros opcionais.</summary>
public sealed class ListCreditInquiriesUseCase(
    ICreditInquiryRepository creditInquiryRepository) : IListCreditInquiriesUseCase
{
    public async Task<ListCreditInquiriesResponse> ExecuteAsync(
        ListCreditInquiriesRequest request,
        CancellationToken cancellationToken)
    {
        var (items, total) = await creditInquiryRepository.ListPagedAsync(
            request.BrokerageId,
            request.PolicyHolderCnpj,
            request.Page,
            request.PageSize,
            cancellationToken);

        var responseItems = items
            .Select(item => new CreditInquiryListItemResponse(
                item.Id,
                item.BrokerageId,
                item.PolicyHolderCnpj,
                item.QueriedAt,
                item.ResultsCount,
                item.AvailableResults))
            .ToList();

        return new ListCreditInquiriesResponse(
            responseItems,
            request.Page,
            request.PageSize,
            total);
    }
}
