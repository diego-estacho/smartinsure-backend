using SmartInsure.Application.UseCase.ModelsBase;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Requests;

/// <summary>RN-018 — lista Corretoras com filtro opcional de situação.</summary>
public sealed record ListBrokeragesRequest : PagedRequest
{
    public string? Status { get; init; }
}
