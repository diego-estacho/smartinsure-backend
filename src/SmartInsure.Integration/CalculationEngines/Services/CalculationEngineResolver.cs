using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Integration.CalculationEngines.Services;

/// <summary>
/// RN-023 — Seleção do Motor de Cálculo pela Habilitação: resolve o motor do par
/// Corretora×Seguradora pela Habilitação Ativa, nunca por escolha fixa no código.
/// Recusas: par sem Habilitação Ativa, Seguradora Inativa no catálogo (RN-010) e motor
/// não disponível na plataforma (sem tentativa por outro motor).
/// </summary>
public sealed class CalculationEngineResolver(
    IInsurerEnablementRepository enablementRepository,
    IInsurerRepository insurerRepository,
    IServiceProvider serviceProvider) : ICalculationEngineResolver
{
    public async Task<CalculationEngineResolution> ResolveAsync(
        Guid brokerageId,
        Guid insurerId,
        CancellationToken cancellationToken)
    {
        var insurer = await insurerRepository.GetByIdAsync(insurerId, cancellationToken)
            ?? throw new NotFoundException("Seguradora não encontrada.");

        if (insurer.Status == EInsurerStatus.Inactive)
        {
            // RN-010: Seguradora Inativa fica fora dos fluxos operacionais.
            throw new BusinessRuleException("A seguradora está inativa e fora de operação.");
        }

        var enablement = await enablementRepository.GetByPairAsync(
            brokerageId, insurerId, cancellationToken);

        if (enablement is null || enablement.Status == EInsurerEnablementStatus.Inactive)
        {
            throw new BusinessRuleException("A seguradora não está habilitada para a corretora.");
        }

        var engine = serviceProvider.GetKeyedService<ICalculationEngine>(enablement.CalculationEngine)
            ?? throw new BusinessRuleException(
                "A seguradora não está habilitada para a corretora.");

        return new CalculationEngineResolution(engine, enablement.ConnectionParameters);
    }
}
