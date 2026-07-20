namespace SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ListBrokerageInsurerEnablements.Requests;

/// <summary>Listagem paginada das Habilitações de Seguradora (RN-022), com filtro opcional por Corretora.</summary>
public sealed record ListBrokerageInsurerEnablementsRequest
{
    public Guid? BrokerageId { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
