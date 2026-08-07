namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.UpdateQuotationTax.Responses;

/// <summary>
/// RN-504: valores que a Seguradora devolveu e que passaram a valer na Cotação escolhida — é o que a
/// tela de emissão apresenta depois do ajuste, sem recalcular nada no cliente.
/// </summary>
public sealed record UpdateQuotationTaxResponse
{
    public decimal? Premium { get; init; }

    public decimal? Tax { get; init; }

    public decimal? CommissionPercentage { get; init; }

    public decimal? CommissionValue { get; init; }

    public IReadOnlyList<InstallmentOptionResponse> InstallmentOptions { get; init; } = [];

    public IReadOnlyList<int> PossibleGracePeriodsInDays { get; init; } = [];
}

/// <summary>RN-505: opção de parcelamento informada pela Seguradora.</summary>
public sealed record InstallmentOptionResponse(int Number, string? Description, decimal Value, bool HasInterest);
