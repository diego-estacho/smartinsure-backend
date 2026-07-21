namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Requests;

/// <summary>RN-029: dados de entrada para executar consulta de limites de crédito do tomador.</summary>
/// <param name="BrokerageId">Identificador da Corretora (Pessoa papel Broker).</param>
/// <param name="PolicyHolderCnpj">CNPJ do tomador para consulta (com ou sem formatação).</param>
public sealed record ExecuteCreditInquiryRequest(
    Guid BrokerageId,
    string PolicyHolderCnpj);
