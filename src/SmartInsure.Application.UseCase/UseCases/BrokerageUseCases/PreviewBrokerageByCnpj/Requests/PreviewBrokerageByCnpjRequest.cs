namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Requests;

/// <summary>RN-032 — consulta somente leitura de um CNPJ no cadastro de Corretora.</summary>
public sealed record PreviewBrokerageByCnpjRequest(string Cnpj);
