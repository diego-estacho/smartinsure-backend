using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.UpdateBrokerage;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.UpdateBrokerage.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.BrokerageUseCases.UpdateBrokerage;

/// <summary>RN-054 — Edição de dados complementares da Corretora.</summary>
[Trait("RuleId", "RN-054")]
public class UpdateBrokerageUseCaseTests
{
    private readonly IPersonRepository _repository = Substitute.For<IPersonRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateBrokerageUseCase _useCase;

    public UpdateBrokerageUseCaseTests()
        => _useCase = new UpdateBrokerageUseCase(_repository, _unitOfWork);

    [Fact]
    public async Task Execute_DeveGravarComplementares_SemTocarNaReceita()
    {
        var id = Guid.NewGuid();
        var person = Person.Create("11444777000161", "Alfa Ltda", null, Guid.NewGuid());
        person.AssignRole(EPersonRole.Broker);
        _repository.GetTrackedBrokerageByIdAsync(id, Arg.Any<CancellationToken>()).Returns(person);
        _repository.GetBrokerageByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(new BrokerageDetailsDto(
                id, "11444777000161", "Alfa Ltda", "Alfa Fantasia", "2062",
                "Sociedade Empresária Limitada", true, "Active", "Active",
                "contato@alfa.com.br", null, null,
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), 0, null));

        await _useCase.ExecuteAsync(
            new UpdateBrokerageRequest(id, "Alfa Fantasia", "contato@alfa.com.br", "1140028922", "Marina"),
            CancellationToken.None);

        person.SocialName.Should().Be("Alfa Fantasia");
        person.Name.Should().Be("Alfa Ltda"); // razão social (Receita) intocada
        var role = person.GetRole(EPersonRole.Broker)!;
        role.ContactEmail.Should().Be("contato@alfa.com.br");
        role.ResponsibleName.Should().Be("Marina");
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoCorretoraInexistente()
    {
        _repository.GetTrackedBrokerageByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Person?)null);

        var action = () => _useCase.ExecuteAsync(
            new UpdateBrokerageRequest(Guid.NewGuid(), null, null, null, null), CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }
}
