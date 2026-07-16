using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer.Responses;

namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer.Interfaces;

public interface ICreateInsurerUseCase : IUseCase<CreateInsurerRequest, CreateInsurerResponse>;
