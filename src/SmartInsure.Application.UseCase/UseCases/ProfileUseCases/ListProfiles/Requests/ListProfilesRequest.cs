namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListProfiles.Requests;

public sealed record ListProfilesRequest
{
    /// <summary>Identidade do solicitante, lida do acesso — define o que ele vê (RN-072).</summary>
    public string ExternalIdentity { get; set; } = string.Empty;

    /// <summary>Corretora ativa do acesso corrente, quando houver.</summary>
    public Guid? ActiveBrokerageId { get; set; }

    /// <summary>Tomador ativo do acesso corrente, quando houver.</summary>
    public Guid? ActivePolicyHolderId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }

    /// <summary>Escopo do Perfil pelo nome estável do contrato (System/Brokerage/PolicyHolder).</summary>
    public string? Scope { get; set; }
}
