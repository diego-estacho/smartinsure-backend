using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IProfileRepository : IRepository<Profile>
{
    /// <summary>Perfil pela chave natural (nome) — usado para resolver o Perfil a conceder (RN-012).</summary>
    Task<Profile?> GetByNameAsync(string name, CancellationToken cancellationToken);

    /// <summary>Perfil fixo Administrador do Sistema (Escopo System) — chave natural, nunca o GUID (RN-012).</summary>
    Task<Profile?> GetSystemAdministratorAsync(CancellationToken cancellationToken);

    /// <summary>Perfil fixo Corretor Administrador (Escopo Brokerage, global) — chave natural (RN-036).</summary>
    Task<Profile?> GetBrokerageAdministratorAsync(CancellationToken cancellationToken);
}
