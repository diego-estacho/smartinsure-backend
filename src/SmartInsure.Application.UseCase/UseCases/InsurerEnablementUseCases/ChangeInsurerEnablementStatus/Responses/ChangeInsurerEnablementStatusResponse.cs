namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ChangeInsurerEnablementStatus.Responses;

/// <summary>Dados de saída da alternância de situação da Habilitação.</summary>
public sealed record ChangeInsurerEnablementStatusResponse(Guid Id, string Status);
