using System.Text.Json.Serialization;

namespace SmartInsure.Integration.Bureau.Models;

/// <summary>
/// Resposta do gateway de Bureau (formato ReceitaWS). Somente resposta com
/// Status "OK" é dado cadastral válido (RN-003).
/// </summary>
public sealed record BureauPersonComplementResponse
{
    public const string StatusOk = "OK";

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("cnpj")]
    public string? Cnpj { get; init; }

    [JsonPropertyName("nome")]
    public string? Nome { get; init; }

    [JsonPropertyName("fantasia")]
    public string? Fantasia { get; init; }

    [JsonPropertyName("situacao")]
    public string? Situacao { get; init; }

    [JsonPropertyName("data_situacao")]
    public string? DataSituacao { get; init; }

    [JsonPropertyName("abertura")]
    public string? Abertura { get; init; }

    [JsonPropertyName("natureza_juridica")]
    public string? NaturezaJuridica { get; init; }

    [JsonPropertyName("porte")]
    public string? Porte { get; init; }

    [JsonPropertyName("capital_social")]
    public string? CapitalSocial { get; init; }

    [JsonPropertyName("logradouro")]
    public string? Logradouro { get; init; }

    [JsonPropertyName("numero")]
    public string? Numero { get; init; }

    [JsonPropertyName("complemento")]
    public string? Complemento { get; init; }

    [JsonPropertyName("bairro")]
    public string? Bairro { get; init; }

    [JsonPropertyName("municipio")]
    public string? Municipio { get; init; }

    [JsonPropertyName("uf")]
    public string? Uf { get; init; }

    [JsonPropertyName("cep")]
    public string? Cep { get; init; }

    [JsonPropertyName("telefone")]
    public string? Telefone { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("atividade_principal")]
    public IReadOnlyList<BureauActivity> AtividadePrincipal { get; init; } = [];

    [JsonPropertyName("atividades_secundarias")]
    public IReadOnlyList<BureauActivity> AtividadesSecundarias { get; init; } = [];

    [JsonIgnore]
    public bool IsOk => string.Equals(Status, StatusOk, StringComparison.OrdinalIgnoreCase);
}

/// <summary>Atividade econômica (CNAE) no formato da fonte.</summary>
public sealed record BureauActivity
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }
}
