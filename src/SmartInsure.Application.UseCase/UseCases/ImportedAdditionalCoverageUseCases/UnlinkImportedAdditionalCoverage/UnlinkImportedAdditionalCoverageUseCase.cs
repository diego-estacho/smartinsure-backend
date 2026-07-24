using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.UnlinkImportedAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.UnlinkImportedAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.UnlinkImportedAdditionalCoverage.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.UnlinkImportedAdditionalCoverage.Requests
{
    /// <summary>Desfazer o vínculo de uma Importada — volta a pendente de mapeamento (RN-043). O id vem da rota.</summary>
    public sealed record UnlinkImportedAdditionalCoverageRequest(Guid ImportedCoverageId);
}

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.UnlinkImportedAdditionalCoverage.Responses
{
    public sealed record UnlinkImportedAdditionalCoverageResponse(Guid ImportedCoverageId);
}

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.UnlinkImportedAdditionalCoverage.Interfaces
{
    public interface IUnlinkImportedAdditionalCoverageUseCase
        : IUseCase<UnlinkImportedAdditionalCoverageRequest, UnlinkImportedAdditionalCoverageResponse>;
}

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.UnlinkImportedAdditionalCoverage
{
    /// <summary>RN-043 — o Administrador desfaz o vínculo; a Importada volta a pendente de mapeamento.</summary>
    public sealed class UnlinkImportedAdditionalCoverageUseCase(
        IImportedAdditionalCoverageRepository importedAdditionalCoverageRepository,
        IUnitOfWork unitOfWork) : IUnlinkImportedAdditionalCoverageUseCase
    {
        public async Task<UnlinkImportedAdditionalCoverageResponse> ExecuteAsync(
            UnlinkImportedAdditionalCoverageRequest request, CancellationToken cancellationToken)
        {
            var imported = await importedAdditionalCoverageRepository.GetByIdAsync(request.ImportedCoverageId, cancellationToken)
                ?? throw new NotFoundException("Cobertura Adicional Importada não encontrada.");

            imported.Unlink();
            await unitOfWork.CommitAsync(cancellationToken);

            return new UnlinkImportedAdditionalCoverageResponse(imported.Id);
        }
    }
}
