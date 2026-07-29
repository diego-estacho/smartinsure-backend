using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.CreateAdditionalCoverage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.CreateAdditionalCoverage.Requests;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.CreateAdditionalCoverage.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.CreateAdditionalCoverage.Requests
{
    /// <summary>Criação de Cobertura Adicional canônica pelo Administrador (RN-040).</summary>
    public sealed record CreateAdditionalCoverageRequest(string Name);
}

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.CreateAdditionalCoverage.Responses
{
    public sealed record CreateAdditionalCoverageResponse(Guid Id, string Name, string Status);
}

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.CreateAdditionalCoverage.Interfaces
{
    public interface ICreateAdditionalCoverageUseCase
        : IUseCase<CreateAdditionalCoverageRequest, CreateAdditionalCoverageResponse>;
}

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.CreateAdditionalCoverage
{
    /// <summary>RN-040 — o Administrador cria uma Cobertura Adicional canônica; nome único no catálogo.</summary>
    public sealed class CreateAdditionalCoverageUseCase(
        IAdditionalCoverageRepository additionalCoverageRepository,
        IUnitOfWork unitOfWork) : ICreateAdditionalCoverageUseCase
    {
        public async Task<CreateAdditionalCoverageResponse> ExecuteAsync(
            CreateAdditionalCoverageRequest request, CancellationToken cancellationToken)
        {
            var name = (request.Name ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(name))
            {
                throw new BusinessRuleException("O nome da Cobertura Adicional é obrigatório.");
            }

            if (await additionalCoverageRepository.GetByNameAsync(name, cancellationToken) is not null)
            {
                throw new BusinessRuleException("Já existe uma Cobertura Adicional com esse nome.");
            }

            var coverage = AdditionalCoverage.Create(name);
            await additionalCoverageRepository.AddAsync(coverage, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return new CreateAdditionalCoverageResponse(coverage.Id, coverage.Name, coverage.Status.ToString());
        }
    }
}
