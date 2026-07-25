namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Responses;

/// <summary>
/// RN-032 — retorno somente leitura da consulta de CNPJ: dados da Receita para revisão e o sinal
/// de "já cadastrada" com o atalho para o cadastro existente. Nada aqui foi gravado.
/// </summary>
public sealed record BrokeragePreviewResponse(
    string DocumentNumber,
    string Name,
    string? SocialName,
    string? LegalNatureCode,
    string? LegalNatureName,
    bool? IsPrivateSector,
    bool AlreadyRegistered,
    Guid? ExistingBrokerageId,
    BrokeragePreviewAddressResponse? MainAddress);

public sealed record BrokeragePreviewAddressResponse(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State);
