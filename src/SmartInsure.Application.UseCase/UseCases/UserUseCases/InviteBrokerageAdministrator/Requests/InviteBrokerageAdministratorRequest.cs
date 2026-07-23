namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator.Requests;

/// <summary>RN-036: dados do convite de um Corretor Administrador.</summary>
/// <param name="Name">Nome do convidado.</param>
/// <param name="Email">E-mail do convidado.</param>
/// <param name="BrokerageIds">Corretoras às quais o convidado passa a estar vinculado.</param>
public sealed record InviteBrokerageAdministratorRequest(
    string Name, string Email, IReadOnlyCollection<Guid> BrokerageIds);
