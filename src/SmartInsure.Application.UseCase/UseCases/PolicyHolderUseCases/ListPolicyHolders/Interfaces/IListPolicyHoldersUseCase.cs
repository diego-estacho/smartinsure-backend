using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolders.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolders.Responses;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolders.Interfaces;

public interface IListPolicyHoldersUseCase : IUseCase<ListPolicyHoldersRequest, PagedResponse<PolicyHolderListItemResponse>>
{
}
