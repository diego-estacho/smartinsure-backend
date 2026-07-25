using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.IgnoreImportedAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.IgnoreImportedAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.IgnoreImportedAdditionalCoverage.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.IgnoreImportedAdditionalCoverage.Requests
{
    /// <summary>Ignorar uma Importada que não deve ser mapeada — sai das pendências (RN-043). O id vem da rota.</summary>
    public sealed record IgnoreImportedAdditionalCoverageRequest(Guid ImportedCoverageId);
}

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.IgnoreImportedAdditionalCoverage.Responses
{
    public sealed record IgnoreImportedAdditionalCoverageResponse(Guid ImportedCoverageId, bool Ignored);
}

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.IgnoreImportedAdditionalCoverage.Interfaces
{
    public interface IIgnoreImportedAdditionalCoverageUseCase
        : IUseCase<IgnoreImportedAdditionalCoverageRequest, IgnoreImportedAdditionalCoverageResponse>;
}

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.IgnoreImportedAdditionalCoverage
{
    /// <summary>RN-043 — o Administrador ignora uma Importada; ela sai da Fila de pendências e não é oferecida.</summary>
    public sealed class IgnoreImportedAdditionalCoverageUseCase(
        IImportedAdditionalCoverageRepository importedAdditionalCoverageRepository,
        IUnitOfWork unitOfWork) : IIgnoreImportedAdditionalCoverageUseCase
    {
        public async Task<IgnoreImportedAdditionalCoverageResponse> ExecuteAsync(
            IgnoreImportedAdditionalCoverageRequest request, CancellationToken cancellationToken)
        {
            var imported = await importedAdditionalCoverageRepository.GetByIdAsync(request.ImportedCoverageId, cancellationToken)
                ?? throw new NotFoundException("Cobertura Adicional Importada não encontrada.");

            imported.Ignore();
            await unitOfWork.CommitAsync(cancellationToken);

            return new IgnoreImportedAdditionalCoverageResponse(imported.Id, imported.IsIgnored);
        }
    }
}
