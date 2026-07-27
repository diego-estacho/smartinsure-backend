using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.PersonImports;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj;

/// <summary>RN-052 (revisada) — Consulta de CNPJ para cadastro de Corretora.</summary>
[Trait("RuleId", "RN-052")]
public class PreviewBrokerageByCnpjUseCaseTests
{
    private const string Cnpj = "11444777000161";

    private readonly IPersonRepository _repository = Substitute.For<IPersonRepository>();
    private readonly IPersonBureauImporter _importer = Substitute.For<IPersonBureauImporter>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly PreviewBrokerageByCnpjUseCase _useCase;

    public PreviewBrokerageByCnpjUseCaseTests()
        => _useCase = new PreviewBrokerageByCnpjUseCase(_repository, _importer, _unitOfWork);

    [Fact]
    public async Task Execute_DevePersistirPjSemPapel_QuandoConsultaCnpjNovo()
    {
        _repository.FindBrokeragePreviewByDocumentAsync(Cnpj, Arg.Any<CancellationToken>())
            .Returns((BrokeragePreviewDto?)null);
        _importer.ImportLegalPersonAsync(Cnpj, EPersonRole.Broker, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ImportedFrom(Cnpj));

        var response = await _useCase.ExecuteAsync(
            new PreviewBrokerageByCnpjRequest(Cnpj), CancellationToken.None);

        response.Name.Should().Be("Alfa Ltda");
        response.LegalNatureName.Should().Be("Sociedade Empresária Limitada");
        response.AlreadyRegistered.Should().BeFalse();
        response.ExistingBrokerageId.Should().BeNull();
        // RN-052 revisada: a PJ é persistida (sem papel Corretor) para reuso da próxima consulta.
        await _repository.Received(1).AddAsync(Arg.Any<Person>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveSinalizarJaCadastrada_QuandoCnpjTemPapelCorretor()
    {
        var brokerageId = Guid.NewGuid();
        _repository.FindBrokeragePreviewByDocumentAsync(Cnpj, Arg.Any<CancellationToken>())
            .Returns(PreviewDto(brokerageId, hasBrokerRole: true, DateTime.UtcNow));

        var response = await _useCase.ExecuteAsync(
            new PreviewBrokerageByCnpjRequest(Cnpj), CancellationToken.None);

        response.AlreadyRegistered.Should().BeTrue();
        response.ExistingBrokerageId.Should().Be(brokerageId);
        await _importer.DidNotReceiveWithAnyArgs()
            .ImportLegalPersonAsync(default!, default, default, default);
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveReusarDaBase_QuandoCacheRecenteSemPapel()
    {
        _repository.FindBrokeragePreviewByDocumentAsync(Cnpj, Arg.Any<CancellationToken>())
            .Returns(PreviewDto(Guid.NewGuid(), hasBrokerRole: false, DateTime.UtcNow));

        var response = await _useCase.ExecuteAsync(
            new PreviewBrokerageByCnpjRequest(Cnpj), CancellationToken.None);

        response.AlreadyRegistered.Should().BeFalse();
        response.ExistingBrokerageId.Should().BeNull();
        // RN-014: cache fresco reaproveitado sem novo custo de Birô e sem gravar.
        await _importer.DidNotReceiveWithAnyArgs()
            .ImportLegalPersonAsync(default!, default, default, default);
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveReconsultarBiroSemGravar_QuandoCacheVencidoSemPapel()
    {
        _repository.FindBrokeragePreviewByDocumentAsync(Cnpj, Arg.Any<CancellationToken>())
            .Returns(PreviewDto(Guid.NewGuid(), hasBrokerRole: false, DateTime.UtcNow.AddDays(-100)));
        _importer.ImportLegalPersonAsync(Cnpj, EPersonRole.Broker, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ImportedFrom(Cnpj));

        var response = await _useCase.ExecuteAsync(
            new PreviewBrokerageByCnpjRequest(Cnpj), CancellationToken.None);

        response.AlreadyRegistered.Should().BeFalse();
        // RN-014: após 90 dias reconsulta o Birô só para exibir; nada é gravado.
        await _importer.ReceivedWithAnyArgs(1)
            .ImportLegalPersonAsync(default!, default, default, default);
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoBiroNaoLocaliza()
    {
        _repository.FindBrokeragePreviewByDocumentAsync(Cnpj, Arg.Any<CancellationToken>())
            .Returns((BrokeragePreviewDto?)null);
        _importer.ImportLegalPersonAsync(Cnpj, EPersonRole.Broker, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((PersonBureauImport?)null);

        var action = () => _useCase.ExecuteAsync(
            new PreviewBrokerageByCnpjRequest(Cnpj), CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>();
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    private static BrokeragePreviewDto PreviewDto(Guid personId, bool hasBrokerRole, DateTime lastUpdatedAt)
        => new(
            personId, Cnpj, "Alfa Ltda", "Alfa", "2062", "Sociedade Empresária Limitada",
            true, hasBrokerRole, null, lastUpdatedAt);

    private static PersonBureauImport ImportedFrom(string cnpj)
    {
        var person = Person.Create(cnpj, "Alfa Ltda", "Alfa", Guid.NewGuid());
        person.AddMainAddress(
            "01310100", "Avenida Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        return new PersonBureauImport(person, true, "2062", "Sociedade Empresária Limitada");
    }
}
