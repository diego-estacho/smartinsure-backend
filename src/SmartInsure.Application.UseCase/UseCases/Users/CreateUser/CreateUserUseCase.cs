using SmartInsure.Application.UseCase.Common;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.Users.CreateUser;

public interface ICreateUserUseCase : IUseCase<CreateUserRequest, CreateUserResponse>;

/// <summary>
/// RN-001 — Criação de Usuário: identidade criada primeiro no provedor de identidade;
/// falha ao gravar na plataforma desfaz a identidade (compensação). Nunca existe
/// Usuário sem identidade correspondente no provedor.
/// </summary>
public sealed class CreateUserUseCase(
    IUserRepository userRepository,
    IIdentityProvider identityProvider,
    IUnitOfWork unitOfWork) : ICreateUserUseCase
{
    public async Task<CreateUserResponse> ExecuteAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await userRepository.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("Já existe um usuário com este e-mail na plataforma.");
        }

        if (await identityProvider.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException(
                "Já existe uma identidade com este e-mail no provedor de identidade.");
        }

        var externalIdentity = await identityProvider.CreateIdentityAsync(
            request.Name.Trim(), email, cancellationToken);

        try
        {
            var user = User.Create(request.Name, email, externalIdentity);

            await userRepository.AddAsync(user, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return new CreateUserResponse(user.Id, user.Name, user.Email, user.Status.ToString());
        }
        catch
        {
            await identityProvider.RemoveIdentityAsync(externalIdentity, CancellationToken.None);
            throw;
        }
    }
}
