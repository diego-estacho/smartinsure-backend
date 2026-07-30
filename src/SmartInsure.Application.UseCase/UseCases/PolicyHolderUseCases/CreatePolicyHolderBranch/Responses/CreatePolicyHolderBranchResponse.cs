namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch.Responses;

/// <summary>
/// RN-101: <see cref="BranchId"/> nulo com <see cref="Notice"/> preenchido significa que a
/// matriz permanece cadastrada e utilizável, mas a Filial não foi localizada no Birô — não é
/// um erro de requisição, é um aviso sobre o resultado.
/// </summary>
public sealed record CreatePolicyHolderBranchResponse(
    Guid HeadquartersId,
    Guid? BranchId,
    string? Notice);
