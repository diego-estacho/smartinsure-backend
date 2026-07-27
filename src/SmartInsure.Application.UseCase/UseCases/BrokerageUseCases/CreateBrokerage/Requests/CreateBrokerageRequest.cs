namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Requests;

/// <summary>
/// RN-019 — criação de Corretora na confirmação: CNPJ + dados complementares (nome fantasia e contato,
/// RN-054) e a escolha de ativar ao salvar. Nada é gravado antes desta chamada (a consulta é RN-052).
/// </summary>
public sealed record CreateBrokerageRequest
{
    public string Cnpj { get; init; } = string.Empty;

    public string? SocialName { get; init; }

    public string? ContactEmail { get; init; }

    public string? ContactPhone { get; init; }

    public string? ResponsibleName { get; init; }

    public bool ActivateOnSave { get; init; } = true;
}
