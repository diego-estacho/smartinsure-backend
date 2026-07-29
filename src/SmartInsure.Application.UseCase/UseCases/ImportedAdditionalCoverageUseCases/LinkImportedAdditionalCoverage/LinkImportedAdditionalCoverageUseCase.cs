using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.LinkImportedAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.LinkImportedAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.LinkImportedAdditionalCoverage.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.LinkImportedAdditionalCoverage.Requests
{
    /// <summary>Vincular (ou reatribuir) uma Importada a uma Cobertura Adicional canônica (RN-043). O id da Importada vem da rota.</summary>
    public sealed record LinkImportedAdditionalCoverageRequest(Guid ImportedCoverageId, Guid AdditionalCoverageId);
}

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.LinkImportedAdditionalCoverage.Responses
{
    public sealed record LinkImportedAdditionalCoverageResponse(Guid ImportedCoverageId, Guid AdditionalCoverageId);
}

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.LinkImportedAdditionalCoverage.Interfaces
{
    public interface ILinkImportedAdditionalCoverageUseCase
        : IUseCase<LinkImportedAdditionalCoverageRequest, LinkImportedAdditionalCoverageResponse>;
}

namespace SmartInsure.Application.UseCase.UseCases.ImportedAdditionalCoverageUseCases.LinkImportedAdditionalCoverage
{
    /// <summary>RN-043 — o Administrador vincula (ou reatribui) uma Cobertura Adicional Importada a uma canônica.</summary>
    public sealed class LinkImportedAdditionalCoverageUseCase(
        IImportedAdditionalCoverageRepository importedAdditionalCoverageRepository,
        IAdditionalCoverageRepository additionalCoverageRepository,
        IUnitOfWork unitOfWork) : ILinkImportedAdditionalCoverageUseCase
    {
        public async Task<LinkImportedAdditionalCoverageResponse> ExecuteAsync(
            LinkImportedAdditionalCoverageRequest request, CancellationToken cancellationToken)
        {
            var imported = await importedAdditionalCoverageRepository.GetByIdAsync(request.ImportedCoverageId, cancellationToken)
                ?? throw new NotFoundException("Cobertura Adicional Importada não encontrada.");

            var canonical = await additionalCoverageRepository.GetByIdAsync(request.AdditionalCoverageId, cancellationToken)
                ?? throw new NotFoundException("Cobertura Adicional não encontrada.");

            imported.LinkTo(canonical.Id);
            await unitOfWork.CommitAsync(cancellationToken);

            return new LinkImportedAdditionalCoverageResponse(imported.Id, canonical.Id);
        }
    }
}
