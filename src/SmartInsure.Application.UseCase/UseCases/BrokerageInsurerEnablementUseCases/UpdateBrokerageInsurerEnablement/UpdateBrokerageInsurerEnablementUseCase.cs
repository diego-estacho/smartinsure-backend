using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.UpdateBrokerageInsurerEnablement.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.UpdateBrokerageInsurerEnablement.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.UpdateBrokerageInsurerEnablement.Responses;
using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.UpdateBrokerageInsurerEnablement;

/// <summary>RN-022 — alteração do motor e dos parâmetros de conexão da Habilitação; par e situação não mudam aqui.</summary>
public sealed class UpdateBrokerageInsurerEnablementUseCase(
    IBrokerageInsurerEnablementRepository enablementRepository,
    IUnitOfWork unitOfWork,
    IServiceProvider serviceProvider) : IUpdateBrokerageInsurerEnablementUseCase
{
    public async Task<UpdateBrokerageInsurerEnablementResponse> ExecuteAsync(
        UpdateBrokerageInsurerEnablementRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ECalculationEngine>(request.CalculationEngine, ignoreCase: true, out var engine))
        {
            throw new BusinessRuleException("O motor de cálculo informado não está disponível na plataforma.");
        }

        var engineService = serviceProvider.GetKeyedService<ICalculationEngine>(engine)
            ?? throw new BusinessRuleException("O motor de cálculo informado não está disponível na plataforma.");

        // RN-022 (caso limite): parâmetros de conexão obrigatórios do motor ausentes recusam a gravação.
        engineService.EnsureValidConnectionParameters(request.ConnectionParameters);

        var enablement = await enablementRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Habilitação não encontrada.");

        enablement.UpdateSettings(engine, request.ConnectionParameters);

        enablementRepository.Update(enablement);
        await unitOfWork.CommitAsync(cancellationToken);

        return new UpdateBrokerageInsurerEnablementResponse(
            enablement.Id,
            enablement.BrokerageId,
            enablement.InsurerId,
            enablement.CalculationEngine.ToString(),
            enablement.ConnectionParameters,
            enablement.Status.ToString());
    }
}
