using SmartInsure.Core.Abstractions.Services.Dtos;

namespace SmartInsure.Core.Abstractions.Services;

/// <summary>
/// Contrato de envio de e-mail (ADR-048) — implementado no projeto de mail (MailKit/SMTP).
/// Recebe corpo HTML pronto e anexos em memória; montagem de template é responsabilidade
/// do chamador. Falha de envio é logada com contexto e propagada; o chamador decide se é crítica.
/// </summary>
public interface IMailService
{
    Task SendAsync(MailMessage message, CancellationToken cancellationToken);
}
