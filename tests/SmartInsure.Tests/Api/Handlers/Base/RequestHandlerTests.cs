using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Application.UseCase.Common;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Api.Handlers.Base;

/// <summary>Pipeline central de request (ADR-011): validação → UseCase → mapeamento → resposta.</summary>
public class RequestHandlerTests
{
    private static readonly RequestHandler Handler = new(
        new ExceptionResultResolver(new ProblemResultFactory(), NullLogger<ExceptionResultResolver>.Instance),
        new ProblemResultFactory());

    public sealed record FakeRequest(string Nome);

    public sealed record FakeResponse(string Nome);

    [Fact]
    public async Task TryHandleAsync_DeveRetornar200ComPayloadSemEnvelope_QuandoUseCaseExecutaComSucesso()
    {
        var useCase = Substitute.For<IUseCase<FakeRequest, FakeResponse>>();
        useCase.ExecuteAsync(Arg.Any<FakeRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FakeResponse("ok"));

        var result = await Handler.TryHandleAsync(new DefaultHttpContext(), useCase, new FakeRequest("x"));

        result.Should().BeOfType<Ok<FakeResponse>>()
            .Which.Value.Should().Be(new FakeResponse("ok"));
    }

    [Fact]
    public async Task TryHandleAsync_DeveRetornar400SemExecutarUseCase_QuandoValidacaoFalha()
    {
        var useCase = Substitute.For<IUseCase<FakeRequest, FakeResponse>>();
        var validator = new InlineValidator<FakeRequest>();
        validator.RuleFor(request => request.Nome).NotEmpty().WithMessage("Nome é obrigatório.");

        var result = await Handler.TryHandleAsync(
            new DefaultHttpContext(), useCase, new FakeRequest(string.Empty), validator);

        var problem = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problem.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        problem.ProblemDetails.Should().BeOfType<HttpValidationProblemDetails>()
            .Which.Errors.Should().ContainKey("Nome");
        await useCase.DidNotReceive().ExecuteAsync(Arg.Any<FakeRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryHandleAsync_DeveMapearParaProblemDetails_QuandoUseCaseLancaExcecaoTipada()
    {
        var useCase = Substitute.For<IUseCase<FakeRequest, FakeResponse>>();
        useCase.ExecuteAsync(Arg.Any<FakeRequest>(), Arg.Any<CancellationToken>())
            .Returns<FakeResponse>(_ => throw new BusinessRuleException("Regra impede a operação."));

        var result = await Handler.TryHandleAsync(new DefaultHttpContext(), useCase, new FakeRequest("x"));

        result.Should().BeOfType<ProblemHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    public async Task TryHandleAsync_DeveUsarResultFactoryCustomizada_QuandoInformada()
    {
        var useCase = Substitute.For<IUseCase<FakeRequest, FakeResponse>>();
        useCase.ExecuteAsync(Arg.Any<FakeRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FakeResponse("criado"));

        var result = await Handler.TryHandleAsync(
            new DefaultHttpContext(),
            useCase,
            new FakeRequest("x"),
            validator: null,
            resultFactory: response => Results.Created($"/api/v1/fakes/{response.Nome}", response));

        result.Should().BeOfType<Created<FakeResponse>>()
            .Which.Location.Should().Be("/api/v1/fakes/criado");
    }
}
