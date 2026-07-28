using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.PersonImports;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.PolicyHolderUseCases.CreatePolicyHolderBranch;

/// <summary>
/// RN-052 — cria a Filial na ficha do Tomador: confirma Tomador com papel PolicyHolder,
/// recusa raiz de CNPJ diferente antes de qualquer consulta ao Birô (OPEN-04) e delega
/// o cadastro em cadeia ao IBranchRegistrar.
/// </summary>
public sealed class CreatePolicyHolderBranchUseCaseTests
{
    private const string PolicyHolderCnpj = "11444777000161";
    private const string BranchCnpj = "11444777000242";
    private const string OtherRootCnpj = "12345678000195";

    private static Person NewPolicyHolder()
    {
        var person = Person.Create(PolicyHolderCnpj, "Alfa Ltda", "Alfa", Guid.NewGuid());
        person.AssignRole(EPersonRole.PolicyHolder);
        return person;
    }

    [Fact]
    [Trait("RuleId", "RN-052")]
    public async Task ExecuteAsync_DeveCadastrarFilialEVincularAoTomador()
    {
        var policyHolder = NewPolicyHolder();
        var branchId = Guid.NewGuid();

        var personRepository = Substitute.For<IPersonRepository>();
        var branchRegistrar = Substitute.For<IBranchRegistrar>();

        personRepository.GetByIdWithRolesAsync(policyHolder.Id, Arg.Any<CancellationToken>())
            .Returns(policyHolder);
        branchRegistrar.RegisterAsync(BranchCnpj, Arg.Any<CancellationToken>())
            .Returns(new BranchRegistration(policyHolder.Id, branchId, null));

        var useCase = new CreatePolicyHolderBranchUseCase(personRepository, branchRegistrar);
        var request = new CreatePolicyHolderBranchRequest(policyHolder.Id, BranchCnpj);

        var response = await useCase.ExecuteAsync(request, CancellationToken.None);

        response.HeadquartersId.Should().Be(policyHolder.Id);
        response.BranchId.Should().Be(branchId);
        response.Notice.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-052")]
    public async Task ExecuteAsync_CnpjDeOutraRaiz_DeveSerRecusado()
    {
        var policyHolder = NewPolicyHolder();

        var personRepository = Substitute.For<IPersonRepository>();
        var branchRegistrar = Substitute.For<IBranchRegistrar>();

        personRepository.GetByIdWithRolesAsync(policyHolder.Id, Arg.Any<CancellationToken>())
            .Returns(policyHolder);

        var useCase = new CreatePolicyHolderBranchUseCase(personRepository, branchRegistrar);
        var request = new CreatePolicyHolderBranchRequest(policyHolder.Id, OtherRootCnpj);

        var action = () => useCase.ExecuteAsync(request, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>();
        await branchRegistrar.DidNotReceiveWithAnyArgs().RegisterAsync(default!, default);
    }

    [Fact]
    [Trait("RuleId", "RN-052")]
    public async Task ExecuteAsync_FilialNaoLocalizadaNoBiro_DeveDevolverAviso()
    {
        var policyHolder = NewPolicyHolder();
        const string notice = "CNPJ da filial não localizado na fonte de dados cadastrais.";

        var personRepository = Substitute.For<IPersonRepository>();
        var branchRegistrar = Substitute.For<IBranchRegistrar>();

        personRepository.GetByIdWithRolesAsync(policyHolder.Id, Arg.Any<CancellationToken>())
            .Returns(policyHolder);
        branchRegistrar.RegisterAsync(BranchCnpj, Arg.Any<CancellationToken>())
            .Returns(new BranchRegistration(policyHolder.Id, null, notice));

        var useCase = new CreatePolicyHolderBranchUseCase(personRepository, branchRegistrar);
        var request = new CreatePolicyHolderBranchRequest(policyHolder.Id, BranchCnpj);

        var response = await useCase.ExecuteAsync(request, CancellationToken.None);

        response.HeadquartersId.Should().Be(policyHolder.Id);
        response.BranchId.Should().BeNull();
        response.Notice.Should().Be(notice);
    }
}
