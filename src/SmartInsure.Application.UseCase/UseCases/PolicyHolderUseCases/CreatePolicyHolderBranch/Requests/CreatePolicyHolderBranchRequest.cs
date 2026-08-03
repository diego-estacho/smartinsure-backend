namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch.Requests;

public sealed record CreatePolicyHolderBranchRequest(Guid PolicyHolderId, string DocumentNumber);
