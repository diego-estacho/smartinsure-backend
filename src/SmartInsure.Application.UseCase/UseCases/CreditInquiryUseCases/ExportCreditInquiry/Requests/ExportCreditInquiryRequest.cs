namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExportCreditInquiry.Requests;

/// <summary>
/// RN-201 — exportação do quadro consolidado de uma Consulta de Crédito (.xlsx, síncrona v1):
/// identificada pelo id da consulta persistida (RN-031). Uma linha por Seguradora.
/// </summary>
public sealed record ExportCreditInquiryRequest(Guid Id);
