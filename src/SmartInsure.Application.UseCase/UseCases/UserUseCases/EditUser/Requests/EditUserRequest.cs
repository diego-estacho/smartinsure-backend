namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.EditUser.Requests;

/// <summary>
/// RN-202: edição de Usuário. Nome sempre editável; E-mail só quando informado E o Usuário está
/// Pendente (antes do primeiro acesso). CPF é imutável (RN-082) e não entra aqui.
/// </summary>
/// <param name="UserId">Usuário a editar.</param>
/// <param name="Name">Novo nome (obrigatório).</param>
/// <param name="Email">Novo e-mail; nulo/vazio = não altera o e-mail.</param>
public sealed record EditUserRequest(Guid UserId, string Name, string? Email);
