using Refit;
using SmartInsure.Integration.Bureau.Models;

namespace SmartInsure.Integration.Bureau.Interfaces;

/// <summary>
/// Gateway de Bureau; autenticação Basic userName:password e headers
/// InsuranceCompanyId/Product no handler do HttpClient.
/// </summary>
public interface IBureauApi
{
    /// <summary>Consulta de dados cadastrais. POST por contrato do gateway, mas leitura idempotente.</summary>
    [Post("/api/bureau/GetPersonComplement")]
    Task<BureauPersonComplementResponse> GetPersonComplementAsync(
        [Body] BureauPersonComplementRequest request,
        CancellationToken cancellationToken);
}
