using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Abstractions;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Api.Handlers.Base;

/// <summary>Mapa canônico exceção tipada → ProblemDetails (ADR-012, ADR-022).</summary>
public class ExceptionResultResolverTests
{
    private static readonly ExceptionResultResolver Resolver = new(
        new ProblemResultFactory(),
        NullLogger<ExceptionResultResolver>.Instance);

    [Fact]
    public void Resolve_DeveRetornar404_QuandoNotFoundException()
        => Resolve(new NotFoundException("Recurso não existe."))
            .StatusCode.Should().Be(StatusCodes.Status404NotFound);

    [Fact]
    public void Resolve_DeveRetornar409_QuandoConflictException()
        => Resolve(new ConflictException("Estado conflitante."))
            .StatusCode.Should().Be(StatusCodes.Status409Conflict);

    [Fact]
    public void Resolve_DeveRetornar422_QuandoBusinessRuleException()
        => Resolve(new BusinessRuleException("Regra impede a operação."))
            .StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);

    [Fact]
    public void Resolve_DeveRetornar500SemDetalheInterno_QuandoExcecaoInesperada()
    {
        var problem = Resolve(new InvalidOperationException("detalhe interno sensível"));

        problem.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        problem.ProblemDetails.Detail.Should().NotContain("detalhe interno sensível");
    }

    [Fact]
    public void Resolve_DeveExporMensagemDaExcecaoNoDetail_QuandoErroDeNegocio()
        => Resolve(new BusinessRuleException("Mensagem em pt-BR pronta para o consumidor."))
            .ProblemDetails.Detail.Should().Be("Mensagem em pt-BR pronta para o consumidor.");

    [Fact]
    public void Resolve_DeveEnriquecerComTraceIdECorrelationId_SempreQueGeraProblema()
    {
        var problem = Resolve(new NotFoundException("Recurso não existe."));

        problem.ProblemDetails.Extensions.Should().ContainKey("traceId");
        problem.ProblemDetails.Extensions.Should().ContainKey("correlationId");
    }

    private static ProblemHttpResult Resolve(Exception exception)
        => Resolver.Resolve(new DefaultHttpContext(), exception)
            .Should().BeOfType<ProblemHttpResult>().Subject;
}
