using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap;
using SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;

namespace SmartInsure.Tests.Application.UseCases.ModalityMapUseCases;

/// <summary>RN-033 — Mapa: oferecida só com ≥1 Importada Ativa Confirmada; disponibilidade por ramo.</summary>
[Trait("RuleId", "RN-033")]
public class GetModalityMapUseCaseTests
{
    private readonly IModalityRepository _modalities = Substitute.For<IModalityRepository>();
    private readonly IModalityMappingRepository _mappings = Substitute.For<IModalityMappingRepository>();
    private readonly IImportedModalityRepository _imported = Substitute.For<IImportedModalityRepository>();

    [Fact]
    public async Task Execute_DeveDerivarOfertaEDisponibilidadePorRamo()
    {
        var mapped = Guid.CreateVersion7();
        var unmapped = Guid.CreateVersion7();
        _modalities.ListActiveForMapAsync(Arg.Any<CancellationToken>()).Returns(new List<ModalityListItemDto>
        {
            new(mapped, "Garantia de Execução", Guid.CreateVersion7(), "Contrato", null, "Active"),
            new(unmapped, "Garantia Judicial", Guid.CreateVersion7(), "Judiciais", null, "Active"),
        });
        _mappings.ListConfirmedActiveAsync(Arg.Any<CancellationToken>()).Returns(new List<ConfirmedMappingDto>
        {
            new(mapped, Guid.CreateVersion7(), "Essor", Guid.CreateVersion7(), "Performance", "Public"),
            new(mapped, Guid.CreateVersion7(), "Excelsior", Guid.CreateVersion7(), "Exec", "Private"),
        });
        _imported.ListPendingAsync(Arg.Any<CancellationToken>()).Returns(new List<PendingImportedModalityDto>
        {
            new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Essor", "Nova modalidade", "Public", "Eng", "Grupo"),
        });

        var response = await new GetModalityMapUseCase(_modalities, _mappings, _imported)
            .ExecuteAsync(new GetModalityMapRequest(), CancellationToken.None);

        var offered = response.Modalities.Single(m => m.ModalityId == mapped);
        offered.Offered.Should().BeTrue();
        offered.Insurers.Should().HaveCount(2);
        offered.Branches.Should().BeEquivalentTo(["Private", "Public"]);

        response.Modalities.Single(m => m.ModalityId == unmapped).Offered.Should().BeFalse();
        response.Pending.Should().HaveCount(1);
    }
}
