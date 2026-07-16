using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ListInsurers;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ListInsurers.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;

namespace SmartInsure.Tests.Application.UseCases.InsurerUseCases.ListInsurers;

/// <summary>RN-008 — consulta operacional exclui Inativas; visão completa é do Administrador do Sistema.</summary>
[Trait("RuleId", "RN-008")]
public class ListInsurersUseCaseTests
{
    private readonly IInsurerRepository _repository = Substitute.For<IInsurerRepository>();
    private readonly ListInsurersUseCase _useCase;

    public ListInsurersUseCaseTests()
        => _useCase = new ListInsurersUseCase(_repository);

    private void RepositoryReturns(int page, int pageSize, bool includeInactive)
        => _repository.ListAsync(page, pageSize, includeInactive, Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<InsurerListItemDto>)
                [new InsurerListItemDto(Guid.NewGuid(), "12345678000195", "Alfa S.A.", null, null, "Active")], 1L));

    [Fact]
    public async Task Execute_DeveListarSomenteAtivas_QuandoConsultaPadrao()
    {
        RepositoryReturns(1, 20, includeInactive: false);

        var response = await _useCase.ExecuteAsync(
            new ListInsurersRequest(), CancellationToken.None);

        response.Items.Should().HaveCount(1);
        await _repository.Received(1).ListAsync(1, 20, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveIncluirInativas_QuandoAdministradorPedeVisaoCompleta()
    {
        RepositoryReturns(1, 20, includeInactive: true);

        await _useCase.ExecuteAsync(
            new ListInsurersRequest { IncludeInactive = true, CallerIsSystemAdministrator = true },
            CancellationToken.None);

        await _repository.Received(1).ListAsync(1, 20, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveIgnorarVisaoCompleta_QuandoSolicitanteSemPerfil()
    {
        RepositoryReturns(1, 20, includeInactive: false);

        await _useCase.ExecuteAsync(
            new ListInsurersRequest { IncludeInactive = true, CallerIsSystemAdministrator = false },
            CancellationToken.None);

        await _repository.Received(1).ListAsync(1, 20, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveNormalizarPaginacao_QuandoValoresForaDoIntervalo()
    {
        RepositoryReturns(1, 1, includeInactive: false);

        var response = await _useCase.ExecuteAsync(
            new ListInsurersRequest { Page = 0, PageSize = 0 },
            CancellationToken.None);

        await _repository.Received(1).ListAsync(1, 1, false, Arg.Any<CancellationToken>());
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(1);
    }
}
