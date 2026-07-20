using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.CreateBrokerageInsurerEnablement.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.CreateBrokerageInsurerEnablement.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.CreateBrokerageInsurerEnablement.Responses;
using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.CreateBrokerageInsurerEnablement;

/// <summary>
/// RN-022 — Habilitação de Seguradora para a Corretora: par único, nasce Ativa;
/// Corretora e Seguradora precisam existir nos respectivos cadastros.
/// </summary>
public sealed class CreateBrokerageInsurerEnablementUseCase(
    IBrokerageInsurerEnablementRepository enablementRepository,
    IInsurerRepository insurerRepository,
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork,
    IServiceProvider serviceProvider) : ICreateBrokerageInsurerEnablementUseCase
{
    public async Task<CreateBrokerageInsurerEnablementResponse> ExecuteAsync(
        CreateBrokerageInsurerEnablementRequest request,
        CancellationToken cancellationToken)
    {
        var engine = ResolveEngine(request.CalculationEngine);

        // RN-022 (caso limite): parâmetros de conexão obrigatórios do motor ausentes recusam a gravação.
        engine.EnsureValidConnectionParameters(request.ConnectionParameters);

        _ = await personRepository.GetBrokerageByIdAsync(request.BrokerageId, cancellationToken)
            ?? throw new NotFoundException("Corretora não encontrada.");

        _ = await insurerRepository.GetByIdAsync(request.InsurerId, cancellationToken)
            ?? throw new NotFoundException("Seguradora não encontrada.");

        if (await enablementRepository.PairExistsAsync(request.BrokerageId, request.InsurerId, cancellationToken))
        {
            throw new ConflictException("A seguradora já está habilitada para esta corretora.");
        }

        var enablement = BrokerageInsurerEnablement.Create(
            request.BrokerageId, request.InsurerId, engine.Engine, request.ConnectionParameters);

        await enablementRepository.AddAsync(enablement, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new CreateBrokerageInsurerEnablementResponse(
            enablement.Id,
            enablement.BrokerageId,
            enablement.InsurerId,
            enablement.CalculationEngine.ToString(),
            enablement.ConnectionParameters,
            enablement.Status.ToString());
    }

    private ICalculationEngine ResolveEngine(string calculationEngine)
    {
        if (!Enum.TryParse<ECalculationEngine>(calculationEngine, ignoreCase: true, out var engine))
        {
            throw new BusinessRuleException("O motor de cálculo informado não está disponível na plataforma.");
        }

        return serviceProvider.GetKeyedService<ICalculationEngine>(engine)
            ?? throw new BusinessRuleException("O motor de cálculo informado não está disponível na plataforma.");
    }
}
