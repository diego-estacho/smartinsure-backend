namespace SmartInsure.Application.UseCase.Services.PersonImports;

/// <summary>
/// RN-101: resultado do cadastro em cadeia da Filial. <c>BranchId</c> nulo com <c>Notice</c>
/// preenchido significa que a matriz foi gravada mas a Filial não foi localizada no Birô.
/// </summary>
public sealed record BranchRegistration(Guid HeadquartersId, Guid? BranchId, string? Notice);

/// <summary>
/// RN-101: dado o CNPJ de uma Filial, resolve a matriz pela raiz do CNPJ, importa do Birô
/// a matriz e a Filial quando ausentes, e vincula a Filial à matriz.
/// </summary>
public interface IBranchRegistrar
{
    /// <returns>
    /// <c>null</c> quando a matriz não é localizada — nada é gravado. Do contrário, a matriz
    /// sempre está gravada; <c>BranchId</c> nulo indica que a Filial não foi localizada.
    /// </returns>
    Task<BranchRegistration?> RegisterAsync(string branchCnpj, CancellationToken cancellationToken);
}
