using SmartInsure.Application.UseCase.Services.PersonImports;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch.Interfaces;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch.Requests;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Validators;

namespace SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch;

/// <summary>
/// RN-101/RN-025 — cadastra a Filial a partir da ficha do Tomador. Confirma que a Pessoa existe
/// com papel PolicyHolder e que o CNPJ informado pertence à mesma raiz de 8 dígitos do Tomador
/// antes de qualquer consulta ao Birô (OPEN-04: a consulta tem custo por chamada) — só então
/// delega o cadastro em cadeia ao IBranchRegistrar. ADR-101: a Filial não recebe Papel da Pessoa.
/// </summary>
public sealed class CreatePolicyHolderBranchUseCase(
    IPersonRepository personRepository,
    IBranchRegistrar branchRegistrar) : ICreatePolicyHolderBranchUseCase
{
    public async Task<CreatePolicyHolderBranchResponse> ExecuteAsync(
        CreatePolicyHolderBranchRequest request,
        CancellationToken cancellationToken)
    {
        var policyHolder = await personRepository.GetByIdWithRolesAsync(
            request.PolicyHolderId, cancellationToken);

        if (policyHolder is null || policyHolder.GetRole(EPersonRole.PolicyHolder) is null)
        {
            throw new NotFoundException("Tomador não encontrado.");
        }

        var branchCnpj = CnpjValidator.Normalize(request.DocumentNumber);

        // OPEN-04: a consulta ao Birô tem custo por chamada — recusar a raiz diferente
        // antes de delegar ao registrar, que é quem consultaria o Birô.
        if (branchCnpj[..8] != policyHolder.DocumentNumber[..8])
        {
            throw new BusinessRuleException(
                "O CNPJ informado não pertence à mesma raiz de CNPJ do tomador.");
        }

        // Defensivo: a matriz já existe como o próprio Tomador confirmado acima, então o
        // registrar sempre a encontra pela raiz (GetTrackedByDocumentNumberAsync) sem ir ao
        // Birô por ela — null aqui não é alcançável na prática, mas não é um bug a "corrigir".
        var registration = await branchRegistrar.RegisterAsync(branchCnpj, cancellationToken)
            ?? throw new BusinessRuleException(
                "CNPJ não localizado na fonte de dados cadastrais.");

        return new CreatePolicyHolderBranchResponse(
            registration.HeadquartersId,
            registration.BranchId,
            registration.Notice);
    }
}
