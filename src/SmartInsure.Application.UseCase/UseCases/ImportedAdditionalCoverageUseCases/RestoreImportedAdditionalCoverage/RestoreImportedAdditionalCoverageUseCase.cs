using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.RestoreImportedAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.RestoreImportedAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.RestoreImportedAdditionalCoverage.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.RestoreImportedAdditionalCoverage.Requests
{
    /// <summary>Reativar uma Importada antes Ignorada — volta a poder ser pendente/mapeada (RN-043). O id vem da rota.</summary>
    public sealed record RestoreImportedAdditionalCoverageRequest(Guid ImportedCoverageId);
}

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.RestoreImportedAdditionalCoverage.Responses
{
    public sealed record RestoreImportedAdditionalCoverageResponse(Guid ImportedCoverageId, bool Ignored);
}

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.RestoreImportedAdditionalCoverage.Interfaces
{
    public interface IRestoreImportedAdditionalCoverageUseCase
        : IUseCase<RestoreImportedAdditionalCoverageRequest, RestoreImportedAdditionalCoverageResponse>;
}

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.RestoreImportedAdditionalCoverage
{
    /// <summary>RN-043 — o Administrador reativa uma Importada antes Ignorada.</summary>
    public sealed class RestoreImportedAdditionalCoverageUseCase(
        IImportedAdditionalCoverageRepository importedAdditionalCoverageRepository,
        IUnitOfWork unitOfWork) : IRestoreImportedAdditionalCoverageUseCase
    {
        public async Task<RestoreImportedAdditionalCoverageResponse> ExecuteAsync(
            RestoreImportedAdditionalCoverageRequest request, CancellationToken cancellationToken)
        {
            var imported = await importedAdditionalCoverageRepository.GetByIdAsync(request.ImportedCoverageId, cancellationToken)
                ?? throw new NotFoundException("Cobertura Adicional Importada não encontrada.");

            imported.Restore();
            await unitOfWork.CommitAsync(cancellationToken);

            return new RestoreImportedAdditionalCoverageResponse(imported.Id, imported.IsIgnored);
        }
    }
}
