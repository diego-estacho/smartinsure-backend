namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolderBranches.Responses;

public sealed record ListPolicyHolderBranchesResponse(
    IReadOnlyList<PolicyHolderBranchResponse> Branches);

/// <summary>RN-052: Filial do Tomador — Pessoa jurídica vinculada à matriz, sem Papel próprio.</summary>
public sealed record PolicyHolderBranchResponse(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName);
