namespace SmartInsure.Integration.CalculationEngines.PlugV2;

/// <summary>
/// Configuração de resiliência HTTP da integração PlugV2 (a conexão em si — baseUrl/key — vive em
/// ConnectionParameters, por Habilitação; isto aqui é infraestrutura, ajustável por ambiente via
/// appsettings na seção <see cref="SectionName"/>). Cada Motor de Cálculo (fornecedor) que entrar tem
/// a SUA seção análoga — a configuração é por fornecedor, isolada, nunca global.
/// </summary>
public sealed class PlugV2Options
{
    public const string SectionName = "CalculationEngines:PlugV2";

    /// <summary>
    /// Timeout (segundos) das chamadas NÃO idempotentes (/Cotation, /UpdateProposalTerms): criam/mutam
    /// recurso, então NÃO re-tentam (RN-057). Sem retry, o timeout precisa acomodar a latência real do
    /// gateway numa ÚNICA tentativa — curto demais faria a tentativa falhar sem o resultado. Default 60s.
    /// </summary>
    public int NonIdempotentTimeoutSeconds { get; set; } = 60;
}
