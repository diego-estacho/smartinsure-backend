using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Carter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartInsure.Api.Handlers.Base;
using SmartInsure.Api.Services;
using SmartInsure.Application.UseCase.IoC;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Infra.BackgroundServices;
using SmartInsure.Infra.CrossCutting.IoC;
using SmartInsure.Infra.CrossCutting.Options;
using SmartInsure.Infra.Data;
using SmartInsure.Integration.Bureau;
using SmartInsure.Integration.Casdoor;
using SmartInsure.MailServices;

namespace SmartInsure.Api.Extensions;

public static class BuilderExtensions
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        // Secrets locais fora do versionamento (SECURITY.md, ADR-054).
        builder.Configuration.AddJsonFile(
            $"appsettings.{builder.Environment.EnvironmentName}.Local.json",
            optional: true,
            reloadOnChange: true);

        // ADR-012: payloads exclusivamente em camelCase via System.Text.Json.
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

        builder.Services.AddCrossCutting();
        builder.Services.AddInfraData(builder.Configuration);
        builder.Services.AddApplicationUseCases();
        builder.Services.AddCasdoorIntegration();
        builder.Services.AddBureauIntegration();
        builder.Services.AddMailServices();
        builder.Services.AddBackgroundServices();

        builder.Services.AddCarter();

        builder.AddJwtAuthentication();
        builder.Services.AddAuthorization();

        // ADR-016: política única aberta; restrição de origem é do gateway.
        builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders(CorrelationIdMiddleware.HeaderName)));

        builder.Services.AddHealthChecks();
        builder.Services.AddOpenApi();

        // ADR-040: contrato IDistributedCache; implementação inicial em memória, trocável.
        builder.Services.AddDistributedMemoryCache();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();

        builder.Services.AddSingleton<ProblemResultFactory>();
        builder.Services.AddSingleton<ExceptionResultResolver>();
        builder.Services.AddSingleton<RequestHandler>();

        // ADR-055: OpenTelemetry → Azure Monitor quando a connection string existir no host.
        if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        {
            builder.Services.AddOpenTelemetry().UseAzureMonitor();
        }

        return builder;
    }

    private static void AddJwtAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // ADR-015: chave simétrica compartilhada com o IdP; issuer, audience,
        // lifetime e assinatura sempre validados.
        builder.Services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    RoleClaimType = jwt.RoleClaimType,
                };

                // RN-006: acesso encerrado (denylist) é recusado mesmo com assinatura e
                // lifetime válidos — sessão é da plataforma, não só do token.
                bearer.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var tokenId = context.Principal?.FindFirst("jti")?.Value;

                        if (string.IsNullOrEmpty(tokenId))
                        {
                            return;
                        }

                        var revocationStore = context.HttpContext.RequestServices
                            .GetRequiredService<IAccessTokenRevocationStore>();

                        if (await revocationStore.IsRevokedAsync(
                            tokenId, context.HttpContext.RequestAborted))
                        {
                            context.Fail("Acesso encerrado.");
                        }
                    },
                };
            });
    }
}
