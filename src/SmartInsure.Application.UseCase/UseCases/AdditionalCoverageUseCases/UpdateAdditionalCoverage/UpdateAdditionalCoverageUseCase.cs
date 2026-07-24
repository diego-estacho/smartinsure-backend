using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.UpdateAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.UpdateAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.UpdateAdditionalCoverage.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.UpdateAdditionalCoverage.Requests
{
    /// <summary>Edição do nome da Cobertura Adicional canônica (RN-040). O id vem da rota.</summary>
    public sealed record UpdateAdditionalCoverageRequest(Guid Id, string Name);
}

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.UpdateAdditionalCoverage.Responses
{
    public sealed record UpdateAdditionalCoverageResponse(Guid Id, string Name, string Status);
}

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.UpdateAdditionalCoverage.Interfaces
{
    public interface IUpdateAdditionalCoverageUseCase
        : IUseCase<UpdateAdditionalCoverageRequest, UpdateAdditionalCoverageResponse>;
}

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.UpdateAdditionalCoverage
{
    /// <summary>RN-040 — o Administrador edita o nome canônico; o nome permanece único no catálogo.</summary>
    public sealed class UpdateAdditionalCoverageUseCase(
        IAdditionalCoverageRepository additionalCoverageRepository,
        IUnitOfWork unitOfWork) : IUpdateAdditionalCoverageUseCase
    {
        public async Task<UpdateAdditionalCoverageResponse> ExecuteAsync(
            UpdateAdditionalCoverageRequest request, CancellationToken cancellationToken)
        {
            var coverage = await additionalCoverageRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException("Cobertura Adicional não encontrada.");

            var name = (request.Name ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(name))
            {
                throw new BusinessRuleException("O nome da Cobertura Adicional é obrigatório.");
            }

            var byName = await additionalCoverageRepository.GetByNameAsync(name, cancellationToken);

            if (byName is not null && byName.Id != coverage.Id)
            {
                throw new BusinessRuleException("Já existe uma Cobertura Adicional com esse nome.");
            }

            coverage.Rename(name);
            await unitOfWork.CommitAsync(cancellationToken);

            return new UpdateAdditionalCoverageResponse(coverage.Id, coverage.Name, coverage.Status.ToString());
        }
    }
}
