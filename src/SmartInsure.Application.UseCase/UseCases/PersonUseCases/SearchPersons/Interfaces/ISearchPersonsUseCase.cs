using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Requests;
using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Responses;

namespace SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Interfaces;

public interface ISearchPersonsUseCase
    : IUseCase<SearchPersonsRequest, SearchPersonsResponse>;
