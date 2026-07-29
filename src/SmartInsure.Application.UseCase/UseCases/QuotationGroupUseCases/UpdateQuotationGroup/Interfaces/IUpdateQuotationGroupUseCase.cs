using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Responses;

namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Interfaces;

public interface IUpdateQuotationGroupUseCase
    : IUseCase<UpdateQuotationGroupRequest, UpdateQuotationGroupResponse>;
