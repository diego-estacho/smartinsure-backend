namespace SmartInsure.Core.Exceptions;

/// <summary>
/// Provedor de identidade indisponível — mapeada para 503 pelo resolver central.
/// RN-005: a mensagem de indisponibilidade é distinta da de credenciais incorretas.
/// </summary>
public sealed class IdentityProviderUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
