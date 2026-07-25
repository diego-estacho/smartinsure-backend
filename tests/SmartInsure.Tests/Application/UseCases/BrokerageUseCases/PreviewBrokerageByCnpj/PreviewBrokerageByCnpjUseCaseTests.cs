using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.PersonImports;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.BrokerageUseCases.PreviewBrokerageByCnpj;

/// <summary>RN-052 — Consulta de CNPJ para cadastro de Corretora (somente leitura).</summary>
[Trait("RuleId", "RN-052")]
public class PreviewBrokerageByCnpjUseCaseTests
{
    private const string Cnpj = "11444777000161";

    private readonly IPersonRepository _repository = Substitute.For<IPersonRepository>();
    private readonly IPersonBureauImporter _importer = Substitute.For<IPersonBureauImporter>();
    private readonly PreviewBrokerageByCnpjUseCase _useCase;

    public PreviewBrokerageByCnpjUseCaseTests()
        => _useCase = new PreviewBrokerageByCnpjUseCase(_repository, _importer);

    [Fact]
    public async Task Execute_NaoDeveGravarNada_QuandoConsultaCnpjNovo()
    {
        _repository.FindBrokeragePreviewByDocumentAsync(Cnpj, Arg.Any<CancellationToken>())
            .Returns((BrokeragePreviewDto?)null);
        _importer.ImportLegalPersonAsync(Cnpj, EPersonRole.Broker, Arg.Any<CancellationToken>())
            .Returns(ImportedFrom(Cnpj));

        var response = await _useCase.ExecuteAsync(
            new PreviewBrokerageByCnpjRequest(Cnpj), CancellationToken.None);

        response.Name.Should().Be("Alfa Ltda");
        response.LegalNatureName.Should().Be("Sociedade Empresária Limitada");
        response.AlreadyRegistered.Should().BeFalse();
        // RN-052: a consulta é somente leitura — nada é gravado.
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Execute_DeveSinalizarJaCadastrada_QuandoCnpjTemPapelCorretor()
    {
        var brokerageId = Guid.NewGuid();
        _repository.FindBrokeragePreviewByDocumentAsync(Cnpj, Arg.Any<CancellationToken>())
            .Returns(new BrokeragePreviewDto(
                brokerageId, Cnpj, "Alfa Ltda", "Alfa", "2062", "Sociedade Empresária Limitada",
                true, HasBrokerRole: true, null));

        var response = await _useCase.ExecuteAsync(
            new PreviewBrokerageByCnpjRequest(Cnpj), CancellationToken.None);

        response.AlreadyRegistered.Should().BeTrue();
        response.ExistingBrokerageId.Should().Be(brokerageId);
        await _importer.DidNotReceiveWithAnyArgs().ImportLegalPersonAsync(default!, default, default);
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoBiroNaoLocaliza()
    {
        _repository.FindBrokeragePreviewByDocumentAsync(Cnpj, Arg.Any<CancellationToken>())
            .Returns((BrokeragePreviewDto?)null);
        _importer.ImportLegalPersonAsync(Cnpj, EPersonRole.Broker, Arg.Any<CancellationToken>())
            .Returns((PersonBureauImport?)null);

        var action = () => _useCase.ExecuteAsync(
            new PreviewBrokerageByCnpjRequest(Cnpj), CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>();
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    private static PersonBureauImport ImportedFrom(string cnpj)
    {
        var person = Person.Create(cnpj, "Alfa Ltda", "Alfa", Guid.NewGuid());
        person.AddMainAddress(
            "01310100", "Avenida Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        return new PersonBureauImport(person, true, "2062", "Sociedade Empresária Limitada");
    }
}
