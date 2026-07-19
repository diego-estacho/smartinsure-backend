using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.CreateInsurerEnablement.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.CreateInsurerEnablement.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.CreateInsurerEnablement.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.CreateInsurerEnablement;

/// <summary>
/// RN-022 — Habilitação de Seguradora para a Corretora: par único, nasce Ativa;
/// Corretora e Seguradora precisam existir nos respectivos cadastros.
/// </summary>
public sealed class CreateInsurerEnablementUseCase(
    IInsurerEnablementRepository enablementRepository,
    IInsurerRepository insurerRepository,
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork) : ICreateInsurerEnablementUseCase
{
    public async Task<CreateInsurerEnablementResponse> ExecuteAsync(
        CreateInsurerEnablementRequest request,
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

        var enablement = InsurerEnablement.Create(
            request.BrokerageId, request.InsurerId, engine, request.ConnectionParameters);

        await enablementRepository.AddAsync(enablement, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new CreateInsurerEnablementResponse(
            enablement.Id,
            enablement.BrokerageId,
            enablement.InsurerId,
            enablement.CalculationEngine.ToString(),
            enablement.ConnectionParameters,
            enablement.Status.ToString());
    }
}
