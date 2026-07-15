namespace SmartInsure.Core.Abstractions.Services.Dtos;

/// <summary>Anexo em memória (ADR-048): o serviço de mail nunca lê disco.</summary>
public sealed record MailAttachment
{
    public required string FileName { get; init; }

    public required byte[] Content { get; init; }

    /// <summary>MIME type do anexo (ex.: application/pdf).</summary>
    public required string ContentType { get; init; }
}
