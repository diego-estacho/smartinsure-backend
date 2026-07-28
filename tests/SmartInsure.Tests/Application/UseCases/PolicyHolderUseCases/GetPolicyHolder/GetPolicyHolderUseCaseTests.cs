using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.GetPolicyHolder;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.GetPolicyHolder.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.PolicyHolderUseCases.GetPolicyHolder;

/// <summary>
/// RN-025 — a ficha do Tomador carrega as Filiais vinculadas à matriz (RN-052).
/// Cobre apenas a composição de <c>branches[]</c> na resposta; o restante do detalhe do
/// Tomador não é retro-coberto aqui.
/// </summary>
public sealed class GetPolicyHolderUseCaseTests
{
    private const string PolicyHolderCnpj = "11444777000161";
    private const string BranchCnpj = "11444777000242";

    [Fact]
    [Trait("RuleId", "RN-025")]
    public async Task ExecuteAsync_DeveIncluirFiliaisDaMatrizNaFicha()
    {
        var policyHolderId = Guid.NewGuid();
        var details = new PolicyHolderDetailsDto(
            policyHolderId,
            PolicyHolderCnpj,
            "Alfa Ltda",
            "Alfa",
            null,
            null,
            true,
            [],
            []);
        var branch = new PersonBranchDto(Guid.NewGuid(), BranchCnpj, "Alfa Filial", "Alfa Filial SA");

        var personRepository = Substitute.For<IPersonRepository>();
        personRepository.GetPolicyHolderByIdAsync(policyHolderId, Arg.Any<CancellationToken>())
            .Returns(details);
        personRepository.ListBranchesAsync(policyHolderId, Arg.Any<CancellationToken>())
            .Returns(new[] { branch });

        var useCase = new GetPolicyHolderUseCase(personRepository);
        var request = new GetPolicyHolderRequest(policyHolderId);

        var response = await useCase.ExecuteAsync(request, CancellationToken.None);

        response.Branches.Should().ContainSingle();
        response.Branches[0].Id.Should().Be(branch.Id);
        response.Branches[0].DocumentNumber.Should().Be(branch.DocumentNumber);
        response.Branches[0].Name.Should().Be(branch.Name);
        response.Branches[0].SocialName.Should().Be(branch.SocialName);
    }
}
