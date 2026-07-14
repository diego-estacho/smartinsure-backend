using System.Globalization;
using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Localization;
using Scalar.AspNetCore;

namespace SmartInsure.Api.Extensions;

public static class AppExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        // ADR-019: cultura fixa pt-BR, sem negociação por Accept-Language.
        var ptBr = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = ptBr;
        CultureInfo.DefaultThreadCurrentUICulture = ptBr;
        ValidatorOptions.Global.LanguageManager.Culture = ptBr;

        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(ptBr),
            SupportedCultures = [ptBr],
            SupportedUICultures = [ptBr],
            RequestCultureProviders = [],
        });

        app.UseCorrelationId();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/health").AllowAnonymous();

        // ADR-018: OpenAPI + Scalar somente em desenvolvimento e QA; produção não expõe.
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("QA"))
        {
            app.MapOpenApi().AllowAnonymous();
            app.MapScalarApiReference().AllowAnonymous();
        }

        // ADR-010: grupo único api/v1 com autorização por default (fail-closed);
        // AllowAnonymous é opt-out explícito por rota.
        var apiV1 = app.MapGroup("api/v1").RequireAuthorization();
        apiV1.MapCarter();

        return app;
    }
}
