namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolders.Requests;

public sealed record ListPolicyHoldersRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }

    /// <summary>
    /// RN-104: Corretora ativa. Quando informada, cada Tomador vem com a indicação
    /// "já é Tomador desta Corretora" (Nomeação Vigente). Ausente na listagem geral.
    /// </summary>
    public Guid? BrokerageId { get; set; }
}
