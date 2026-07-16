using SmartInsure.Application.UseCase.ModelsBase;

namespace SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ListInsurers.Requests;

/// <summary>Consulta do catálogo (RN-010): visão completa só surte efeito para o Administrador do Sistema.</summary>
public sealed record ListInsurersRequest : PagedRequest
{
    public bool IncludeInactive { get; init; }

    public bool CallerIsSystemAdministrator { get; init; }
}
