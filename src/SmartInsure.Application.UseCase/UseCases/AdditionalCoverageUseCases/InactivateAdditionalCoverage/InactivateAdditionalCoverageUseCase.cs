using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.InactivateAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.InactivateAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.InactivateAdditionalCoverage.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.InactivateAdditionalCoverage.Requests
{
    /// <summary>Inativação da Cobertura Adicional canônica (RN-040/RN-046). O id vem da rota.</summary>
    public sealed record InactivateAdditionalCoverageRequest(Guid Id);
}

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.InactivateAdditionalCoverage.Responses
{
    public sealed record InactivateAdditionalCoverageResponse(Guid Id, string Status);
}

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.InactivateAdditionalCoverage.Interfaces
{
    public interface IInactivateAdditionalCoverageUseCase
        : IUseCase<InactivateAdditionalCoverageRequest, InactivateAdditionalCoverageResponse>;
}

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.InactivateAdditionalCoverage
{
    /// <summary>RN-040/RN-046 — o Administrador inativa a Cobertura Adicional canônica (não é excluída).</summary>
    public sealed class InactivateAdditionalCoverageUseCase(
        IAdditionalCoverageRepository additionalCoverageRepository,
        IUnitOfWork unitOfWork) : IInactivateAdditionalCoverageUseCase
    {
        public async Task<InactivateAdditionalCoverageResponse> ExecuteAsync(
            InactivateAdditionalCoverageRequest request, CancellationToken cancellationToken)
        {
            var coverage = await additionalCoverageRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException("Cobertura Adicional não encontrada.");

            coverage.Deactivate();
            await unitOfWork.CommitAsync(cancellationToken);

            return new InactivateAdditionalCoverageResponse(coverage.Id, coverage.Status.ToString());
        }
    }
}
