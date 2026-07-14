# Arquitetura — SmartInsure Backend

> O inventário de tecnologias e padrões está em [docs/BACKEND.md](docs/BACKEND.md) (fonte da verdade da stack); este documento descreve a organização — camadas, módulos, estrutura de pastas, pipeline e testes. As decisões arquiteturais individuais estão formalizadas em ADRs em [docs/adr/](docs/adr/) (índice em [docs/adr/index.md](docs/adr/index.md)).

## Visão geral

Backend .NET 10 (C# 14: nullable habilitado, primary constructors, file-scoped namespaces como padrão único), estruturado em Clean Architecture com padrão UseCase. Dois deployáveis: uma API HTTP stateless (Carter/Minimal APIs, sem MVC Controllers) e um app Azure Functions (isolated worker) para tarefas agendadas via Timer Trigger. Persistência principal em SQL Server via EF Core 10; MongoDB para logs, payloads e auditoria. Schema de banco gerenciado por Flyway em repositório dedicado, com migrations imutáveis (forward-only). Processamento assíncrono in-process via Channel + BackgroundService com reconciliador periódico. Serialização JSON exclusivamente com System.Text.Json (camelCase). Autenticação JWT Bearer contra IdP OIDC (Casdoor). Observabilidade com OpenTelemetry exportando para Azure Monitor / Application Insights, com CorrelationId propagado entre chamadas. Documentação de API via OpenAPI + Scalar (exposta em desenvolvimento e QA; desligada em produção). CI/CD por branch (develop/qa/master) no GitHub Actions. Convenção de nomes da solution: `<Produto>.<Módulo>`.

## Camadas e regra de dependência

```
0 Presentation   Api | Functions          → Application, Core, Infra.*, Integration, Services, Providers (composition roots dos dois deployáveis)
1 Application    Application.UseCase      → Core, CrossCutting, Integration, MailServices, Services
2 Domain         Core                     → (nada — camada mais interna, sem dependências externas)
3 Infra          Infra.Data | Infra.CrossCutting | Infra.BackgroundServices   → Core
4 Services       Integration | MailServices | Services.<Motor>                            → Core
5 Providers      Providers.<Fornecedor>   → Core, Services.<Motor> (implementa o contrato do motor)
```

- Sentido único: camadas externas dependem das internas; o Core não referencia ninguém.
- Api e Functions são os composition roots (um por deployável): todo o wiring de DI (nativa do .NET) acontece neles; cada módulo expõe seu método `Add*`.
- Contratos (interfaces de repositório, `IUnitOfWork`, serviços, channels) vivem no Core; implementações vivem na Infra — inversão de dependência.
- Gate automatizado: as regras acima são verificadas por testes de arquitetura (NetArchTest).

## Módulos da solution

| Módulo                     | Camada       | Responsabilidade                                                                                                                                                                                              |
| -------------------------- | ------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Api`                      | Presentation | Endpoints Carter, pipeline HTTP, auth/policies, plumbing de resposta/erro, health checks, OpenAPI/Scalar, composition root. Deployável 1.                                                                     |
| `Application.UseCase`      | Application  | UseCases (um por ação), validators FluentValidation, handlers de eventos, registro de DI dos usecases.                                                                                                        |
| `Core`                     | Domain       | Entidades ricas (setters privados, invariantes no domínio), enums, domain events, exceções tipadas de negócio, contratos (repositórios, UoW, serviços, channels), DTOs de leitura. Sem dependências externas. |
| `Infra.Data`               | Infra        | DbContext EF Core, mappings Fluent, repositórios + UnitOfWork, repositório Mongo genérico.                                                                                                                    |
| `Infra.CrossCutting`       | Infra        | Identidade/JWT, classes Options, validadores de documento, redação de dados sensíveis em log.                                                                                                                 |
| `Infra.BackgroundServices` | Infra        | Channel bounded + BackgroundService consumidor + reconciliador periódico; controle de concorrência para recursos externos.                                                                                    |
| `Functions`                | Presentation | App Azure Functions (isolated worker): Timer Triggers para tarefas agendadas, delegando a services/usecases. Composition root próprio. Deployável 2.                                                         |
| `Integration`              | Services     | Clients HTTP de terceiros (SSO, captcha, CEP, bureau, pagamento) — uma pasta por integração, cada uma com sua classe Options.                                                                                 |
| `Services.<Motor>`         | Services     | Abstração/orquestração de provedores: contratos, resolução por tipo de engine. Sem I/O externo.                                                                                                               |
| `Providers.<Fornecedor>`   | Providers    | Adaptador de fornecedor: Refit, mappers como camada anticorrupção (ACL), factory de client, contratos de integração.                                                                                          |
| `MailServices`             | Services     | Envio de e-mail SMTP (MailKit/MimeKit); recebe corpo HTML pronto e anexos em memória.                                                                                                                         |
| `Tests`                    | Testes       | Unitários (xUnit + NSubstitute + FluentAssertions) + testes de arquitetura (NetArchTest); espelham a estrutura de `src`.                                                                                      |

Repositório separado de migrations (Flyway): scripts versionados `V{yyyyMMddHHmmss}__descricao-kebab.sql` (timestamp puro, `outOfOrder` desabilitado), imutáveis, aplicados por GitHub Actions no fluxo de PR develop → qa → master.

## Estrutura de pastas por módulo

```
Api/
├── Endpoints/            # 1 Carter module por entidade: {Entidade}Endpoint.cs
├── Handlers/Base/        # RequestHandler (pipeline), ProblemResultFactory, ExceptionResultResolver
├── Extensions/           # BuilderExtensions (DI), AppExtensions (middleware), auth helpers, claims
├── Services/             # serviços exclusivos da borda (claims transformation, cache de identidade)
├── Program.cs            # minimal hosting, sem Startup
└── appsettings.{Ambiente}.json  (+ appsettings.{Ambiente}.Local.json gitignored p/ secrets locais)

Application.UseCase/
├── UseCases/{Agregado}UseCases/{Ação}/
│   ├── {Ação}UseCase.cs          # sealed class, primary constructor
│   ├── Interfaces/I{Ação}UseCase.cs
│   ├── Requests/  Responses/  Validators/
├── Events/               # handlers de domain events e publicação de integration events
├── Common/               # IUseCase<TReq,TRes>, Unit, extensions de validação, guards
├── ModelsBase/           # PagedRequest, PagedResponse
└── IoC/DependencyInjection.cs  # assembly scanning por convenção (I{Ação}UseCase → {Ação}UseCase; validators por assembly)

Core/
├── Entities/             # + subpasta p/ documentos Mongo (logs/auditoria)
├── Events/               # domain events (fatos do negócio, sem contratos externos)
├── Enumerators/          # prefixo E, persistidos como string
├── Abstractions/
│   ├── IUnitOfWork.cs
│   ├── Repositories/     # I{Entidade}Repository : IRepository<T>
│   ├── Repositories/Dtos/  # read-models sealed record (projeção, nunca entidades EF na borda)
│   ├── Services/  Channels/  Events/
├── Exceptions/           # exceções tipadas de negócio (NotFound, Conflict, BusinessRule)
├── Constants/            # classes estáticas só p/ constantes (Roles, Policies, CacheKeys, Routes)
└── Settings/  Extensions/

Infra.Data/
├── Context/              # DbContext único (atua como Unit of Work)
├── Mappings/             # {Entidade}Mapping : IEntityTypeConfiguration<T>, assembly scan
├── Repositories/         # 1 repo por agregado + UnitOfWork + MongoRepository<T>
└── DependencyInjection.cs

Infra.CrossCutting/       # Identity/, Options/, Validators/, Logging/, IoC/
Infra.BackgroundServices/ # Channels/, Services/ (consumidor + reconciliador), DependencyInjection.cs
Functions/                # {Tarefa}Function.cs (Timer Trigger), Services/, Program.cs
Integration/              # {Integração}/{Interfaces,Models,Services,Options}
Providers.<Fornecedor>/   # Refit/, Mappers/ (ACL), Factory/, Options/, DependencyInjection.cs
```

## Pipeline de requisição (síncrono)

Endpoint Carter (`{Entidade}Endpoint`, prefixo de rota no construtor, handlers private static) → `RequestHandler.TryHandleAsync`: validação FluentValidation → execução do UseCase → mapeamento de exceções → resposta. Grupo único `api/v1` com `RequireAuthorization` por default; `AllowAnonymous` é opt-out explícito (webhooks autenticam por header secreto). API stateless; payloads em camelCase. Erros seguem RFC 9457 (ProblemDetails) enriquecidos com TraceId e CorrelationId, com códigos HTTP conforme a semântica do protocolo (nunca conforme a regra de negócio): 400 validação de request (`ValidationProblemDetails`) e request malformado; 401 não autenticado; 403 autenticado sem permissão; 404 recurso inexistente; 409 conflito de estado; 422 regra de negócio impede a operação; 500 erro inesperado. Erro de negócio é sinalizado pelo UseCase por exceções tipadas (`NotFoundException`, `ConflictException`, `BusinessRuleException`), mapeadas 1:1 para o ProblemDetails correspondente no resolver central. UseCases não se compõem entre si — lógica compartilhada é extraída para serviço. Cultura do pipeline fixa em pt-BR (produto mono-idioma). Sucesso retorna o payload sem envelope; listagens usam `PagedRequest`/`PagedResponse`. `CancellationToken` propagado em toda operação assíncrona.

## Eventos

- Domain Events (Core): representam fatos relevantes do negócio; disparados após alterações válidas do domínio; nunca carregam contratos externos. Handlers vivem na Application.
- Integration Events: comunicação com sistemas externos; são contratos de integração e não pertencem ao domínio — definidos e publicados fora do Core (Application/Providers).

## Processamento assíncrono e agendado

- Operações longas retornam 202 Accepted e enfileiram um work item num `Channel<T>` bounded (abstraído no Core); um `BackgroundService` consome com scope de DI próprio por item e controle de concorrência para recursos externos.
- Acompanhamento pelo cliente por polling em endpoint de status (sem push — coerente com a API stateless).
- Todo processamento em memória tem reconciliador periódico (recupera itens perdidos em restart/deploy); operações assíncronas são idempotentes, e retry só existe para operações idempotentes.
- Tarefas agendadas vivem no app Azure Functions com Timer Trigger, delegando a services/usecases e auditando execução no Mongo.

## Persistência

- SQL Server (EF Core 10): DbContext único atuando como Unit of Work; mapeamento 100% Fluent API em `Mappings/` (entidades do domínio sem atributos de persistência); FKs com `DeleteBehavior.Restrict` por convenção global; isolamento multi-tenant por Global Query Filters alimentados por accessors de contexto (anuláveis para jobs/sistema); `AsNoTracking` em leituras; projeção direta para DTOs de leitura (entidades EF nunca atravessam a borda da API); enums persistidos como string. Escrita sempre via entidade rastreada + `SaveChanges` — SQL cru de escrita é proibido (bulk excepcional é migration Flyway).
- Identidade: chave única UUIDv7 (`Guid.CreateVersion7()`).
- Auditoria: toda entidade herda a base de auditoria (CreatedAt/UpdatedAt/CreatedBy/UpdatedBy), preenchida automaticamente por `SaveChangesInterceptor`; datas persistidas em UTC; precisão monetária explícita nos mapeamentos.
- MongoDB: exclusivo para dados não relacionais (logs de integração, payloads de erro, auditoria de tarefas) via `IMongoRepository<T>` genérico; nunca para agregados de negócio.
- Cache: `Microsoft.Extensions.Caching.Distributed` (inicia com MemoryDistributedCache, trocável sem mudar contrato).
- Schema: Flyway é o único dono (EF Migrations proibidas); migrations imutáveis — correção é sempre migration nova.

## Configuração (Options Pattern)

Toda configuração externa usa Options Pattern: uma classe Options por integração, representando exatamente uma seção do appsettings; registro via `AddOptions().BindConfiguration()` com `ValidateOnStart()`; `IOptions<T>` é o padrão (`IOptionsSnapshot`/`IOptionsMonitor` só com necessidade explícita). Classes estáticas ficam restritas a constantes da aplicação — nunca para leitura de configuração. Secrets: local via `appsettings.{Ambiente}.Local.json` (gitignored); produção via variáveis de ambiente do host; nenhum secret versionado.

## Integrações externas (estilo)

- Client HTTP padrão: Refit + HttpClientFactory + `AddStandardResilienceHandler` (retry exponencial, circuit breaker, timeout) em todo client, sem exceção. Base URL dinâmica por configuração quando o destino varia por parceiro (Provider Factory + named client).
- Motor multi-fornecedor: `Services.<Motor>` define o contrato e resolve a implementação por tipo de engine (Provider Strategy); `Providers.<Fornecedor>` implementa com mappers atuando como camada anticorrupção (ACL) — o modelo do parceiro não vaza para o domínio.
- SSO: JWT Bearer validado contra IdP OIDC; enriquecimento de claims por `IClaimsTransformation` com cache; policies fail-closed.
- Webhooks inbound: autenticação por header secreto; requests logados no Mongo.

## Observabilidade e resiliência

OpenTelemetry (traces, métricas e logs) exportando para Azure Monitor / Application Insights; CorrelationId propagado entre chamadas e devolvido nos ProblemDetails junto com TraceId; redação de dados sensíveis nos logs (Data Redaction); health checks via `Microsoft.Extensions.Diagnostics.HealthChecks` expostos pela API.

## Testes

- Unitários (`Tests`): xUnit + NSubstitute + FluentAssertions + coverlet; estrutura espelha `src` (`UseCases/{Agregado}UseCases/{Ação}/`); nomenclatura `Metodo_Resultado_Condicao`; TDD (Red → Green → Refactor); fakes/stubs apenas para fronteiras externas; cobertura prioriza casos de uso críticos; repositórios não têm teste dedicado.
- Testes de arquitetura (NetArchTest) no projeto de unitários, travando a regra de dependência entre camadas como gate — sem Skip permanente.
- Testes data-driven (golden scenarios) para massas de cenários canônicos.
- Testes de integração fora do escopo inicial (risco aceito e registrado em ADR: mapeamentos e query filters sem verificação contra banco real); quando adotados, o caminho é WebApplicationFactory in-process com fronteiras substituídas em `ConfigureTestServices`.

## CI/CD e ambientes

- GitHub Actions: build + testes (`dotnet test`) como gate obrigatório de PR; deploy por push nas branches develop/qa/master, cada uma amarrada ao ambiente correspondente.
- Hospedagem: API em Azure App Service (publish Release self-contained linux-x64); app Azure Functions como deployável separado no mesmo fluxo por branch.
- Configuração por ambiente: `appsettings.{Ambiente}.json` versionado sem secrets + `Local.json` para dev.
- Migrations: workflow próprio no repo de migrations, disparado por push em `migrations/**`, aplicando Flyway no banco do ambiente correspondente à branch.
