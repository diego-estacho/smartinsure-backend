using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ActivateAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ActivateAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ActivateAdditionalCoverage.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ActivateAdditionalCoverage.Requests
{
    /// <summary>Ativação da Cobertura Adicional canônica (RN-040/RN-046). O id vem da rota.</summary>
    public sealed record ActivateAdditionalCoverageRequest(Guid Id);
}

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ActivateAdditionalCoverage.Responses
{
    public sealed record ActivateAdditionalCoverageResponse(Guid Id, string Status);
}

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ActivateAdditionalCoverage.Interfaces
{
    public interface IActivateAdditionalCoverageUseCase
        : IUseCase<ActivateAdditionalCoverageRequest, ActivateAdditionalCoverageResponse>;
}

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ActivateAdditionalCoverage
{
    /// <summary>RN-040/RN-046 — o Administrador ativa a Cobertura Adicional canônica.</summary>
    public sealed class ActivateAdditionalCoverageUseCase(
        IAdditionalCoverageRepository additionalCoverageRepository,
        IUnitOfWork unitOfWork) : IActivateAdditionalCoverageUseCase
    {
        public async Task<ActivateAdditionalCoverageResponse> ExecuteAsync(
            ActivateAdditionalCoverageRequest request, CancellationToken cancellationToken)
        {
            var coverage = await additionalCoverageRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException("Cobertura Adicional não encontrada.");

            coverage.Activate();
            await unitOfWork.CommitAsync(cancellationToken);

            return new ActivateAdditionalCoverageResponse(coverage.Id, coverage.Status.ToString());
        }
    }
}
