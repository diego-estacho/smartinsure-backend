using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages;

/// <summary>RN-018 — Pessoas jurídicas com papel Corretor, filtradas por situação opcional.</summary>
public sealed class ListBrokeragesUseCase(IPersonRepository personRepository) : IListBrokeragesUseCase
{
    public async Task<PagedResponse<BrokerageListItemResponse>> ExecuteAsync(
        ListBrokeragesRequest request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var status = ParseStatus(request.Status);

        var (items, totalCount) = await personRepository.ListBrokeragesAsync(
            page, pageSize, status, cancellationToken);

        var responses = items
            .Select(item => new BrokerageListItemResponse(
                item.Id,
                item.DocumentNumber,
                item.Name,
                item.SocialName,
                item.IsPrivateSector,
                item.Status))
            .ToList();

        return new PagedResponse<BrokerageListItemResponse>(
            responses, page, pageSize, totalCount);
    }

    private static EPersonRoleStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return Enum.TryParse<EPersonRoleStatus>(status, ignoreCase: true, out var parsed)
            ? parsed
            : throw new BusinessRuleException("A situação deve ser Active ou Inactive.");
    }
}
