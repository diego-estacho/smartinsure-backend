using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolders;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.ListPolicyHolders.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.PolicyHolderUseCases.ListPolicyHolders;

[Trait("Category", "UseCase")]
public sealed class ListPolicyHoldersUseCaseTests
{
    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();

    [Fact]
    [Trait("RuleId", "RN-025")]
    public async Task Execute_DeveListarApenasComPapelTomador()
    {
        var request = new ListPolicyHoldersRequest { Page = 1, PageSize = 20 };
        var dto1 = new PolicyHolderListItemDto(Guid.NewGuid(), "12345678901234", "Empresa 1", "E1", true);
        var dto2 = new PolicyHolderListItemDto(Guid.NewGuid(), "98765432101234", "Empresa 2", "E2", false);
        _personRepository.ListPolicyHoldersAsync(1, 20, null, CancellationToken.None)
            .Returns((new[] { dto1, dto2 }, 2L));

        var useCase = new ListPolicyHoldersUseCase(_personRepository);
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.TotalCount.Should().Be(2);
        await _personRepository.Received(1).ListPolicyHoldersAsync(1, 20, null, CancellationToken.None);
    }

    [Fact]
    [Trait("RuleId", "RN-025")]
    public async Task Execute_DeveAplicarFiltroSearch_QuandoInformado()
    {
        var request = new ListPolicyHoldersRequest { Page = 1, PageSize = 20, Search = "Empresa" };
        var dto = new PolicyHolderListItemDto(Guid.NewGuid(), "12345678901234", "Empresa Test", null, true);
        _personRepository.ListPolicyHoldersAsync(1, 20, "Empresa", CancellationToken.None)
            .Returns((new[] { dto }, 1L));

        var useCase = new ListPolicyHoldersUseCase(_personRepository);
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        await _personRepository.Received(1).ListPolicyHoldersAsync(1, 20, "Empresa", CancellationToken.None);
    }

    [Fact]
    [Trait("RuleId", "RN-025")]
    public async Task Execute_DeveRespeitarPaginacao()
    {
        var request = new ListPolicyHoldersRequest { Page = 2, PageSize = 50 };
        _personRepository.ListPolicyHoldersAsync(2, 50, null, CancellationToken.None)
            .Returns((Array.Empty<PolicyHolderListItemDto>(), 0L));

        var useCase = new ListPolicyHoldersUseCase(_personRepository);
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        result.Page.Should().Be(2);
        await _personRepository.Received(1).ListPolicyHoldersAsync(2, 50, null, CancellationToken.None);
    }
}
