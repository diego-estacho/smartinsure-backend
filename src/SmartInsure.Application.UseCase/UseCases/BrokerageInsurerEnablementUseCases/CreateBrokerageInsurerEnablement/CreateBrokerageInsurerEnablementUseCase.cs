using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.CreateBrokerageInsurerEnablement.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.CreateBrokerageInsurerEnablement.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.CreateBrokerageInsurerEnablement.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
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
    IUnitOfWork unitOfWork) : ICreateBrokerageInsurerEnablementUseCase
{
    public async Task<CreateBrokerageInsurerEnablementResponse> ExecuteAsync(
        CreateBrokerageInsurerEnablementRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ECalculationEngine>(request.CalculationEngine, ignoreCase: true, out var engine))
        {
            throw new BusinessRuleException("O motor de cálculo informado não está disponível na plataforma.");
        }

        _ = await personRepository.GetBrokerageByIdAsync(request.BrokerageId, cancellationToken)
            ?? throw new NotFoundException("Corretora não encontrada.");

        _ = await insurerRepository.GetByIdAsync(request.InsurerId, cancellationToken)
            ?? throw new NotFoundException("Seguradora não encontrada.");

        if (await enablementRepository.PairExistsAsync(request.BrokerageId, request.InsurerId, cancellationToken))
        {
            throw new ConflictException("A seguradora já está habilitada para esta corretora.");
        }

        var enablement = BrokerageInsurerEnablement.Create(
            request.BrokerageId, request.InsurerId, engine, request.ConnectionParameters);

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
}
