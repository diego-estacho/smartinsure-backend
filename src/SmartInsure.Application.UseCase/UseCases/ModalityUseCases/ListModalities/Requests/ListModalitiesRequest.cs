using SmartInsure.Application.UseCase.ModelsBase;

namespace SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ListModalities.Requests;

/// <summary>Consulta de Modalidades (RN-036/RN-039): visão completa só surte efeito para o Administrador do Sistema.</summary>
public sealed record ListModalitiesRequest : PagedRequest
{
    public bool IncludeInactive { get; init; }

    public bool CallerIsSystemAdministrator { get; init; }
}
