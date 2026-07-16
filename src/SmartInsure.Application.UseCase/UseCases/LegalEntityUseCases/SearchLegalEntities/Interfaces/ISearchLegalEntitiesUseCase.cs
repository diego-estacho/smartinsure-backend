using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.LegalEntityUseCases.SearchLegalEntities.Requests;
using SmartInsure.Application.UseCase.UseCases.LegalEntityUseCases.SearchLegalEntities.Responses;

namespace SmartInsure.Application.UseCase.UseCases.LegalEntityUseCases.SearchLegalEntities.Interfaces;

public interface ISearchLegalEntitiesUseCase
    : IUseCase<SearchLegalEntitiesRequest, SearchLegalEntitiesResponse>;
