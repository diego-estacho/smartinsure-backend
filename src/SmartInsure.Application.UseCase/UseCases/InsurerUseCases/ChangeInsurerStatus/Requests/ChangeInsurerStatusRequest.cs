namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Requests;

/// <summary>Dados de entrada para alterar a situação de uma Seguradora (RN-007).</summary>
/// <param name="InsurerId">Identificador da seguradora a ser alterada.</param>
/// <param name="Status">Situação desejada pelo nome estável: Active ou Inactive.</param>
public sealed record ChangeInsurerStatusRequest(
    Guid InsurerId,
    string Status);
