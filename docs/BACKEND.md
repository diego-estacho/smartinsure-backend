# Convenções do backend — stack e padrões

> Somente nomes; uso e justificativa ficam em [ARCHITECTURE.md](../ARCHITECTURE.md) e nas ADRs ([índice](adr/index.md)).

## Plataforma

- .NET 10 (C# 14)
- ASP.NET Core Minimal APIs
- Azure Functions isolated worker (segundo deployável — tarefas agendadas via Timer Trigger)

## Frameworks e bibliotecas

- Carter (módulos de endpoint)
- Entity Framework Core 10 (provider SQL Server)
- MongoDB.Driver
- FluentValidation / ValidationProblemDetails
- Refit + Refit.HttpClientFactory
- Microsoft.Extensions.Http.Resilience
- MailKit / MimeKit (SMTP)
- Casdoor SDK (Casdoor.Client / Casdoor.AspNetCore)
- Microsoft.AspNetCore.Authentication.JwtBearer
- System.Text.Json (serializador único)
- Scalar
- OpenTelemetry -> Azure Monitor / Application Insights
- System.Threading.Channels (Channel bounded + BackgroundService consumidor + reconciliador periódico)
- Microsoft.Extensions.Caching.Distributed (inicia com MemoryDistributedCache)
- Microsoft.Extensions.Diagnostics.HealthChecks

## Armazenamento e dados

- SQL Server (Azure SQL)
- MongoDB (Atlas)
- Flyway (Redgate) — repositório de migrations dedicado

## Infraestrutura e operação

- Azure App Service (hospedagem da API)
- Azure Functions
- GitHub Actions (CI/CD por branch)
- Casdoor (IdP OIDC/SSO)
- Azure Monitor / Application Insights

## Testes

- xUnit
- NSubstitute
- FluentAssertions
- coverlet (cobertura)
- Microsoft.AspNetCore.Mvc.Testing (WebApplicationFactory — caminho futuro dos testes de integração, fora do escopo inicial)
- NetArchTest

## Padrões arquiteturais

- Clean Architecture (dependências apontando para o domínio)

- Composition root na API (DI nativa do .NET)

- UseCase pattern (Application Service)

- Repository pattern (um por agregado)

- Unit of Work (DbContext)

- Integration Patterns
  
  - Provider Strategy
  
  - Provider Factory
  
  - Anti-Corruption Layer (ACL)

- DTO / read-model (projeção, nunca entidades EF)

- Options Pattern
  
  - Toda configuração externa deve utilizar Options Pattern.
  - Cada integração possui sua própria classe Options.
  - Todas as Options são registradas via AddOptions().BindConfiguration().
  - Todas as configurações devem utilizar ValidateOnStart().
  - Cada classe Options representa exatamente uma seção do appsettings.
  - IOptions<T> é o padrão.
  - IOptionsSnapshot<T> e IOptionsMonitor<T> são utilizados apenas quando houver necessidade explícita.
  - Classes estáticas ficam restritas a constantes da aplicação (Roles, Policies, CacheKeys, Routes etc.), nunca para leitura de configuração.

- Dependency Injection (registro de UseCases/validators por assembly scanning)

- Exceções tipadas de erro de negócio (NotFound / Conflict / BusinessRule) → ProblemDetails

- Entidades ricas (setters privados, invariantes no domínio); sem Specification pattern

- Integration Events
  
  - Enviados para sistemas externos.
  
  - Representam comunicação entre sistemas.
  
  - Representam contratos de integração.
  
  - Não pertencem ao domínio.

- Domain Events
  
  - Representam fatos relevantes do negócio.
  - São disparados após alterações válidas do domínio.
  - Nunca carregam contratos externos.

## Padrões de API

- REST com versionamento por rota (`api/v1`)
- RFC 9457 ProblemDetails enriquecido com TraceId e CorrelationId.
- Códigos HTTP conforme a semântica do protocolo (400/401/403/404/409/422/500 — mapa canônico na ADR-012), nunca conforme regra de negócio.
- Paginação padrão (PagedRequest/PagedResponse)
- Autorização por default no grupo de rotas; AllowAnonymous como exceção explícita
- Policies de autorização fail-closed
- Claims transformation (enriquecimento de identidade com cache)
- Autenticação de webhook por header secreto
- FluentValidation -> ValidationProblemDetails
- API Stateless.
- Naming: camelCase

## Padrões de dados

- Mapeamento Fluent API (IEntityTypeConfiguration, assembly scan)
- Global Query Filters (isolamento multi-tenant)
- AsNoTracking em leituras
- Enum persistido como string
- FK com DeleteBehavior.Restrict (convenção global)
- Identidade única: **UUIDv7** via `Guid.CreateVersion7()`
- Migrations imutáveis (forward-only), versionadas por timestamp puro (`outOfOrder` desabilitado)
- Auditoria automática de entidades (base obrigatória + SaveChangesInterceptor)
- Escrita sempre via change tracker (SQL cru de escrita proibido)
- Datas persistidas em UTC
- Precisão monetária explícita

## Padrões de processamento assíncrono

- Azure Functions (Timer Trigger para tarefas agendadas)
- Producer-Consumer via Channel<T> para processamento assíncrono em memória.
- Acompanhamento de operação assíncrona por polling em endpoint de status (sem push).
- Processamentos em memória possuem reconciliador periódico.
- 202 Accepted para operações longas
- Processamento idempotente para operações assíncronas.
- Retry apenas para operações idempotentes.
- Controle de concorrência para recursos externos.
- CancellationToken propagado em toda operação assíncrona.

## Padrões de resiliência e segurança

- Retry exponencial + Circuit Breaker + Timeout (Standard Resilience Handler)
- JWT Bearer (OIDC via Casdoor)
- Redação de dados sensíveis em logs (Data Redaction)
- CorrelationId propagado entre chamadas.

## Padrões de teste

- TDD (Red → Green → Refactor)
- Estrutura de testes espelhando `src`
- Nomenclatura `Metodo_Resultado_Condicao`
- Fakes/Stubs apenas para fronteiras externas.
- Testes de arquitetura como gate (fitness functions)
- Testes data-driven (golden scenarios)
- Cobertura prioriza casos de uso críticos.
