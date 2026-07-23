using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap;
using SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;

namespace SmartInsure.Tests.Application.UseCases.ModalityMapUseCases;

/// <summary>RN-036 — Mapa: oferecida só com ≥1 Importada Ativa vinculada; Seguradora distinta com contagem; ramo derivado.</summary>
[Trait("RuleId", "RN-036")]
public class GetModalityMapUseCaseTests
{
    private readonly IModalityRepository _modalities = Substitute.For<IModalityRepository>();
    private readonly IImportedModalityRepository _imported = Substitute.For<IImportedModalityRepository>();

    [Fact]
    public async Task Execute_DeveDerivarOfertaDisponibilidadePorRamoEAgregarSeguradoras()
    {
        var mapped = Guid.CreateVersion7();
        var unmapped = Guid.CreateVersion7();
        var essor = Guid.CreateVersion7();
        var excelsior = Guid.CreateVersion7();

        _modalities.ListActiveForMapAsync(Arg.Any<CancellationToken>()).Returns(new List<ModalityListItemDto>
        {
            new(mapped, "Garantia de Execução", null, "Active"),
            new(unmapped, "Garantia Judicial", null, "Active"),
        });
        _imported.ListActiveLinksAsync(Arg.Any<CancellationToken>()).Returns(new List<ModalityInsurerLinkDto>
        {
            // Duas Importadas da mesma Seguradora (Essor) sustentam a Modalidade — uma badge, contagem 2.
            new(mapped, essor, "Essor", "Performance", "Public"),
            new(mapped, essor, "Essor", "Execução PF", "Private"),
            new(mapped, excelsior, "Excelsior", "Exec", "Private"),
        });
        _imported.ListPendingAsync(Arg.Any<CancellationToken>()).Returns(new List<PendingImportedModalityDto>
        {
            new(Guid.CreateVersion7(), Guid.CreateVersion7(), "Essor", "Sem global", "Public", null, "Grupo"),
        });

        var response = await new GetModalityMapUseCase(_modalities, _imported)
            .ExecuteAsync(new GetModalityMapRequest(), CancellationToken.None);

        var offered = response.Modalities.Single(m => m.ModalityId == mapped);
        offered.Offered.Should().BeTrue();
        offered.Insurers.Should().HaveCount(2);
        offered.Insurers.Single(i => i.InsurerId == essor).Count.Should().Be(2);
        offered.Insurers.Single(i => i.InsurerId == essor).Origins.Should().BeEquivalentTo(["Performance", "Execução PF"]);
        offered.Branches.Should().BeEquivalentTo(["Private", "Public"]);

        response.Modalities.Single(m => m.ModalityId == unmapped).Offered.Should().BeFalse();
        response.Pending.Should().HaveCount(1);
    }
}
