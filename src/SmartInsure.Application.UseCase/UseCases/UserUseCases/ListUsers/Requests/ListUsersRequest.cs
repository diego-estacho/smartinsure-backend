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

    /// <summary>Situação do Usuário: Active/Inactive/Pending/Expired (Pending/Expired recortam o Convite).</summary>
    public string? Status { get; set; }

    /// <summary>Filtro avançado (§4): Perfil de acesso.</summary>
    public Guid? ProfileId { get; set; }

    /// <summary>Filtro avançado (§4): Escopo do Perfil (System/Brokerage/PolicyHolder).</summary>
    public string? Scope { get; set; }

    /// <summary>Filtro avançado (§4): Vínculo (Corretora/Tomador).</summary>
    public Guid? LinkId { get; set; }

    /// <summary>Filtro avançado (§4): data de cadastro a partir de.</summary>
    public DateTime? RegisteredFrom { get; set; }

    /// <summary>Filtro avançado (§4): data de cadastro até.</summary>
    public DateTime? RegisteredTo { get; set; }
}
