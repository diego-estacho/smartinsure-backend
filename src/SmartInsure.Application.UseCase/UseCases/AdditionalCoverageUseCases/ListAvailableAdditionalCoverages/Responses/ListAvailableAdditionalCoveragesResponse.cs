namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ListAvailableAdditionalCoverages.Responses;

/// <summary>RN-104: Coberturas Adicionais canônicas ofertáveis para a Modalidade (união simples).</summary>
public sealed record ListAvailableAdditionalCoveragesResponse(
    IReadOnlyList<AvailableAdditionalCoverageItemResponse> Items);

/// <summary>Uma Cobertura Adicional canônica ofertável: o que o corretor vê e escolhe.</summary>
public sealed record AvailableAdditionalCoverageItemResponse(Guid Id, string Name);
