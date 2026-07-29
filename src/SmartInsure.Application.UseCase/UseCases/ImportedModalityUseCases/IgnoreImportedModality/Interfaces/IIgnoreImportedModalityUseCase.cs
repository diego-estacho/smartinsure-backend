using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Responses;

namespace SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Interfaces;

public interface IIgnoreImportedModalityUseCase
    : IUseCase<IgnoreImportedModalityRequest, IgnoreImportedModalityResponse>;
