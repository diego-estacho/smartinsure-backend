using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Responses;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.UpdateBrokerage.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.UpdateBrokerage.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.UpdateBrokerage;

/// <summary>
/// RN-054 — edita os dados complementares da Corretora (nome fantasia e contato). Não altera os dados
/// obtidos do Birô (razão social, Natureza Jurídica, endereço), que seguem import-once (RN-014). A
/// situação apresentada (RN-053) é recalculada no retorno.
/// </summary>
public sealed class UpdateBrokerageUseCase(
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork) : IUpdateBrokerageUseCase
{
    public async Task<GetBrokerageResponse> ExecuteAsync(
        UpdateBrokerageRequest request,
        CancellationToken cancellationToken)
    {
        var person = await personRepository.GetTrackedBrokerageByIdAsync(request.BrokerageId, cancellationToken)
            ?? throw new NotFoundException("Corretora não encontrada.");

        person.UpdateBrokerageComplementaryData(
            request.SocialName,
            request.ContactEmail,
            request.ContactPhone,
            request.ResponsibleName);

        await unitOfWork.CommitAsync(cancellationToken);

        var brokerage = await personRepository.GetBrokerageByIdAsync(request.BrokerageId, cancellationToken)
            ?? throw new NotFoundException("Corretora não encontrada.");

        return GetBrokerageResponse.From(brokerage);
    }
}
