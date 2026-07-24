using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Responses;

namespace SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Interfaces;

public interface ICreateQuotationGroupUseCase
    : IUseCase<CreateQuotationGroupRequest, CreateQuotationGroupResponse>;
