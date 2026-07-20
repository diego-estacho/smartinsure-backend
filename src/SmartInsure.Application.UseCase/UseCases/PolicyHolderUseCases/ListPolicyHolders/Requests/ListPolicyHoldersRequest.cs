namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolders.Requests;

public sealed record ListPolicyHoldersRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }
}
