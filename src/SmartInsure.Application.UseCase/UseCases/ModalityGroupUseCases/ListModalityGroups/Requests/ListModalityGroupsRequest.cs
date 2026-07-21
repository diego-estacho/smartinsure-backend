using SmartInsure.Application.UseCase.ModelsBase;

namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ListModalityGroups.Requests;

/// <summary>Consulta de Grupos (RN-036): visão completa só surte efeito para o Administrador do Sistema.</summary>
public sealed record ListModalityGroupsRequest : PagedRequest
{
    public bool IncludeInactive { get; init; }

    public bool CallerIsSystemAdministrator { get; init; }
}
