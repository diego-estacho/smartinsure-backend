namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ChangeInsurerEnablementStatus.Requests;

/// <summary>Alternância de situação da Habilitação (RN-022), pelo nome estável Active/Inactive.</summary>
public sealed record ChangeInsurerEnablementStatusRequest(Guid Id, string Status);
