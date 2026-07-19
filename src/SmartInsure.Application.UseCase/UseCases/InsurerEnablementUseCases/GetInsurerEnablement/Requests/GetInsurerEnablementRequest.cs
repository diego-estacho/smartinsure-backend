namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.GetInsurerEnablement.Requests;

/// <summary>Consulta de uma Habilitação de Seguradora pelo identificador (RN-022).</summary>
public sealed record GetInsurerEnablementRequest(Guid Id);
