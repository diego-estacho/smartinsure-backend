using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.PersonImports;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.CreateBrokerage.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.BrokerageUseCases.CreateBrokerage;

/// <summary>RN-019 — Criação de Corretora por CNPJ.</summary>
[Trait("RuleId", "RN-019")]
public class CreateBrokerageUseCaseTests
{
    private const string Cnpj = "11444777000161";

    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly ILegalNatureRepository _legalNatureRepository = Substitute.For<ILegalNatureRepository>();
    private readonly IBureauProvider _bureauProvider = Substitute.For<IBureauProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateBrokerageUseCase _useCase;

    public CreateBrokerageUseCaseTests()
    {
        _useCase = new CreateBrokerageUseCase(
            _personRepository,
            new PersonBureauImporter(_legalNatureRepository, _bureauProvider),
            _unitOfWork);

        _personRepository.GetBrokerageByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(call => Brokerage((Guid)call[0]!));
    }

    private static BrokerageDetailsDto Brokerage(Guid id)
        => new(
            id,
            Cnpj,
            "Alfa Ltda",
            "Alfa",
            "2062",
            "Sociedade Empresária Limitada",
            true,
            "Active",
            null);

    private static BureauPersonComplement Complement()
        => new()
        {
            Name = "Alfa Ltda",
            TradeName = "Alfa",
            LegalNature = "206-2 - Sociedade Empresária Limitada",
        };

    [Fact]
    public async Task Execute_DeveAdicionarPapelBroker_QuandoPessoaJuridicaJaExisteSemCorretor()
    {
        var person = Person.Create(Cnpj, "Alfa Ltda", "Alfa", Guid.NewGuid());
        _personRepository.GetTrackedByDocumentNumberAsync(Cnpj, Arg.Any<CancellationToken>())
            .Returns(person);

        var response = await _useCase.ExecuteAsync(new CreateBrokerageRequest(Cnpj), CancellationToken.None);

        response.Status.Should().Be("Active");
        person.Roles.Should().ContainSingle(role => role.Role == EPersonRole.Broker
            && role.Status == EPersonRoleStatus.Active);
        await _bureauProvider.DidNotReceiveWithAnyArgs()
            .GetPersonComplementAsync(default!, default!, default, default);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoPessoaJaTemPapelBroker()
    {
        var person = Person.Create(Cnpj, "Alfa Ltda", "Alfa", Guid.NewGuid());
        person.AssignRole(EPersonRole.Broker);
        _personRepository.GetTrackedByDocumentNumberAsync(Cnpj, Arg.Any<CancellationToken>())
            .Returns(person);

        var action = () => _useCase.ExecuteAsync(new CreateBrokerageRequest(Cnpj), CancellationToken.None);

        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("Corretora já cadastrada.");
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveRecusarSemAlterarSituacao_QuandoPessoaJaTemPapelBrokerInativo()
    {
        var person = Person.Create(Cnpj, "Alfa Ltda", "Alfa", Guid.NewGuid());
        person.AssignRole(EPersonRole.Broker);
        person.GetRole(EPersonRole.Broker)!.Deactivate();
        _personRepository.GetTrackedByDocumentNumberAsync(Cnpj, Arg.Any<CancellationToken>())
            .Returns(person);

        var action = () => _useCase.ExecuteAsync(new CreateBrokerageRequest(Cnpj), CancellationToken.None);

        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("Corretora já cadastrada.");
        person.GetRole(EPersonRole.Broker)!.Status.Should().Be(EPersonRoleStatus.Inactive);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveImportarPessoaJuridica_QuandoCnpjNaoExiste()
    {
        _personRepository.GetTrackedByDocumentNumberAsync(Cnpj, Arg.Any<CancellationToken>())
            .Returns((Person?)null);
        _bureauProvider.GetPersonComplementAsync(Cnpj, "Corretor", EBureau.ReceitaWS, Arg.Any<CancellationToken>())
            .Returns(Complement());
        _legalNatureRepository.GetByCodeAsync("2062", Arg.Any<CancellationToken>())
            .Returns(LegalNature.Create(2018, "2062", "Sociedade Empresária Limitada", true));

        var response = await _useCase.ExecuteAsync(new CreateBrokerageRequest(Cnpj), CancellationToken.None);

        response.DocumentNumber.Should().Be(Cnpj);
        await _personRepository.Received(1).AddAsync(
            Arg.Is<Person>(person => person.DocumentNumber == Cnpj
                && person.Roles.Any(role => role.Role == EPersonRole.Broker
                    && role.Status == EPersonRoleStatus.Active)),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_NaoDeveCriar_QuandoBiroNaoLocaliza()
    {
        _personRepository.GetTrackedByDocumentNumberAsync(Cnpj, Arg.Any<CancellationToken>())
            .Returns((Person?)null);
        _bureauProvider.GetPersonComplementAsync(Cnpj, "Corretor", EBureau.ReceitaWS, Arg.Any<CancellationToken>())
            .Returns((BureauPersonComplement?)null);

        var action = () => _useCase.ExecuteAsync(new CreateBrokerageRequest(Cnpj), CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>();
        await _personRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }
}
