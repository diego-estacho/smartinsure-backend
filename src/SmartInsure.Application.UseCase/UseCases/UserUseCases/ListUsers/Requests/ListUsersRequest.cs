namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Requests;

public sealed record ListUsersRequest
{
    /// <summary>Identidade do solicitante, lida do acesso — define o que ele pode ver (RN-064).</summary>
    public string ExternalIdentity { get; set; } = string.Empty;

    /// <summary>Corretora ativa do acesso corrente, quando houver.</summary>
    public Guid? ActiveBrokerageId { get; set; }

    /// <summary>Tomador ativo do acesso corrente, quando houver.</summary>
    public Guid? ActivePolicyHolderId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }

    /// <summary>Situação do Usuário pelo nome estável do contrato (Pending/Active/Inactive).</summary>
    public string? Status { get; set; }
}
