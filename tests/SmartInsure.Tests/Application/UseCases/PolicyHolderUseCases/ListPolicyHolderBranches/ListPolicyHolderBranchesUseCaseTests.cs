using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolderBranches;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolderBranches.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.PolicyHolderUseCases.ListPolicyHolderBranches;

/// <summary>
/// RN-101 — lista as Filiais cadastradas e vinculadas ao Tomador (matriz), confirmando
/// antes que a Pessoa existe com papel PolicyHolder.
/// </summary>
public sealed class ListPolicyHolderBranchesUseCaseTests
{
    private const string PolicyHolderCnpj = "11444777000161";
    private const string BranchCnpj = "11444777000242";

    private static Person NewPolicyHolder()
    {
        var person = Person.Create(PolicyHolderCnpj, "Alfa Ltda", "Alfa", Guid.NewGuid());
        person.AssignRole(EPersonRole.PolicyHolder);
        return person;
    }

    [Fact]
    [Trait("RuleId", "RN-101")]
    public async Task ExecuteAsync_DeveListarFiliaisDaMatriz()
    {
        var policyHolder = NewPolicyHolder();
        var branch = new PersonBranchDto(Guid.NewGuid(), BranchCnpj, "Alfa Filial", "Alfa Filial SA");

        var personRepository = Substitute.For<IPersonRepository>();
        personRepository.GetByIdWithRolesAsync(policyHolder.Id, Arg.Any<CancellationToken>())
            .Returns(policyHolder);
        personRepository.ListBranchesAsync(policyHolder.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { branch });

        var useCase = new ListPolicyHolderBranchesUseCase(personRepository);
        var request = new ListPolicyHolderBranchesRequest(policyHolder.Id);

        var response = await useCase.ExecuteAsync(request, CancellationToken.None);

        response.Branches.Should().ContainSingle();
        response.Branches[0].Id.Should().Be(branch.Id);
        response.Branches[0].DocumentNumber.Should().Be(branch.DocumentNumber);
        response.Branches[0].Name.Should().Be(branch.Name);
        response.Branches[0].SocialName.Should().Be(branch.SocialName);
    }

    [Fact]
    [Trait("RuleId", "RN-101")]
    public async Task ExecuteAsync_PessoaInexistente_DeveLancarNotFound()
    {
        var policyHolderId = Guid.NewGuid();

        var personRepository = Substitute.For<IPersonRepository>();
        personRepository.GetByIdWithRolesAsync(policyHolderId, Arg.Any<CancellationToken>())
            .Returns((Person?)null);

        var useCase = new ListPolicyHolderBranchesUseCase(personRepository);
        var request = new ListPolicyHolderBranchesRequest(policyHolderId);

        var action = () => useCase.ExecuteAsync(request, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
        await personRepository.DidNotReceiveWithAnyArgs().ListBranchesAsync(default, default);
    }

    [Fact]
    [Trait("RuleId", "RN-101")]
    public async Task ExecuteAsync_PessoaSemPapelTomador_DeveLancarNotFound()
    {
        // Pessoa cadastrada, mas sem AssignRole(PolicyHolder) — não é Tomador.
        var person = Person.Create(PolicyHolderCnpj, "Alfa Ltda", "Alfa", Guid.NewGuid());

        var personRepository = Substitute.For<IPersonRepository>();
        personRepository.GetByIdWithRolesAsync(person.Id, Arg.Any<CancellationToken>())
            .Returns(person);

        var useCase = new ListPolicyHolderBranchesUseCase(personRepository);
        var request = new ListPolicyHolderBranchesRequest(person.Id);

        var action = () => useCase.ExecuteAsync(request, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
        await personRepository.DidNotReceiveWithAnyArgs().ListBranchesAsync(default, default);
    }
}
