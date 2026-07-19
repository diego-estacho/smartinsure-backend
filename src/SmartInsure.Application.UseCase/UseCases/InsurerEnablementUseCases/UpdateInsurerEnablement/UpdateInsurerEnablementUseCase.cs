using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.UpdateInsurerEnablement.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.UpdateInsurerEnablement.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.UpdateInsurerEnablement.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.UpdateInsurerEnablement;

/// <summary>RN-022 — alteração do motor e dos parâmetros de conexão da Habilitação; par e situação não mudam aqui.</summary>
public sealed class UpdateInsurerEnablementUseCase(
    IInsurerEnablementRepository enablementRepository,
    IUnitOfWork unitOfWork) : IUpdateInsurerEnablementUseCase
{
    public async Task<UpdateInsurerEnablementResponse> ExecuteAsync(
        UpdateInsurerEnablementRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ECalculationEngine>(request.CalculationEngine, ignoreCase: true, out var engine))
        {
            throw new BusinessRuleException("O motor de cálculo informado não está disponível na plataforma.");
        }

        var enablement = await enablementRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Habilitação não encontrada.");

        enablement.UpdateSettings(engine, request.ConnectionParameters);

        enablementRepository.Update(enablement);
        await unitOfWork.CommitAsync(cancellationToken);

        return new UpdateInsurerEnablementResponse(
            enablement.Id,
            enablement.BrokerageId,
            enablement.InsurerId,
            enablement.CalculationEngine.ToString(),
            enablement.ConnectionParameters,
            enablement.Status.ToString());
    }
}
