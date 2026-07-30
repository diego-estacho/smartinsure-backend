using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.GetQuotationGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.GetQuotationGroup.Responses;

namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.GetQuotationGroup.Interfaces;

public interface IGetQuotationGroupUseCase
    : IUseCase<GetQuotationGroupRequest, GetQuotationGroupResponse>;
