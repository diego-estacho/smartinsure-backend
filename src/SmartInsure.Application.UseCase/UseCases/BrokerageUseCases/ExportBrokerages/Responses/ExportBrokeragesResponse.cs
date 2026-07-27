namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ExportBrokerages.Responses;

public sealed record ExportBrokeragesResponse(byte[] Content, string FileName, string ContentType);
