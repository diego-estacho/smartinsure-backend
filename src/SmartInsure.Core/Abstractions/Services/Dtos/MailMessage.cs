namespace SmartInsure.Core.Abstractions.Services.Dtos;

/// <summary>
/// Mensagem de e-mail genérica (ADR-048). O remetente é fixo da configuração —
/// nunca informado pelo chamador. <see cref="ReplyTo"/> é opcional para direcionar
/// respostas quando o From é uma caixa no-reply.
/// </summary>
public sealed record MailMessage
{
    public required IReadOnlyList<string> To { get; init; }

    public IReadOnlyList<string> Cc { get; init; } = [];

    public IReadOnlyList<string> Bcc { get; init; } = [];

    public string? ReplyTo { get; init; }

    public required string Subject { get; init; }

    /// <summary>Corpo HTML pronto — montagem de template vive no chamador (ADR-048).</summary>
    public required string HtmlBody { get; init; }

    public IReadOnlyList<MailAttachment> Attachments { get; init; } = [];
}
