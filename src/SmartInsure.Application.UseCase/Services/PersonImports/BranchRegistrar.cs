using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.Services.PersonImports;

/// <summary>
/// RN-052/ADR-063: cadastro em cadeia da Filial. Nem a matriz nem a Filial recebem Papel da
/// Pessoa nesta operação — vincular Papel é responsabilidade de outro fluxo (RN-017); aqui só
/// existência e vínculo. Falha do Birô na matriz não grava nada; falha na Filial preserva a
/// matriz já gravada (o Birô cobra por chamada — OPEN-04 — descartar uma matriz válida para
/// punir a ausência da Filial seria pior).
/// </summary>
public sealed class BranchRegistrar(
    IPersonRepository personRepository,
    IPersonBureauImporter personBureauImporter,
    IUnitOfWork unitOfWork) : IBranchRegistrar
{
    private const string BranchNotFoundNotice =
        "CNPJ da filial não localizado na fonte de dados cadastrais.";

    public async Task<BranchRegistration?> RegisterAsync(
        string branchCnpj, CancellationToken cancellationToken)
    {
        if (CnpjValidator.IsHeadquarters(branchCnpj))
        {
            throw new BusinessRuleException("O CNPJ informado é de matriz, não de filial.");
        }

        var headquartersCnpj = CnpjValidator.HeadquartersOf(branchCnpj);

        var headquarters = await EnsurePersonAsync(headquartersCnpj, cancellationToken);

        // RN-052: matriz não localizada no Birô — nada é gravado.
        if (headquarters is null)
        {
            return null;
        }

        var branch = await EnsurePersonAsync(branchCnpj, cancellationToken);

        // RN-052: filial não localizada — a matriz permanece cadastrada e utilizável.
        if (branch is null)
        {
            return new BranchRegistration(headquarters.Id, null, BranchNotFoundNotice);
        }

        if (branch.HeadquartersPersonId is null)
        {
            branch.LinkToHeadquarters(headquarters);
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return new BranchRegistration(headquarters.Id, branch.Id, null);
    }

    /// <summary>
    /// RN-014: devolve a Pessoa da base ou importa do Birô uma única vez. RN-052/ADR-063: papel
    /// nulo — nem a matriz nem a Filial recebem Papel da Pessoa por esta operação.
    /// </summary>
    private async Task<Person?> EnsurePersonAsync(string cnpj, CancellationToken cancellationToken)
    {
        var existing = await personRepository.GetTrackedByDocumentNumberAsync(cnpj, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var imported = await personBureauImporter.ImportLegalPersonAsync(
            cnpj, role: null, cancellationToken);

        if (imported is null)
        {
            return null;
        }

        await personRepository.AddAsync(imported.Person, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return imported.Person;
    }
}
