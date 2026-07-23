# Índice de ADRs — SmartInsure Backend

> Uma linha por ADR; o resumo condensa a norma. Carregue a ADR completa quando a regra se aplicar ao escopo da task. ADR-001 a ADR-004 são do harness; ADR-005 em diante formalizam a arquitetura do backend (ver [ARCHITECTURE.md](../../ARCHITECTURE.md) e [BACKEND.md](../BACKEND.md)).

| ID | Título | Status | Tags | Resumo |
|----|--------|--------|------|--------|
| [ADR-001](001-camada-de-produto-unica.md) | Camada de produto única, compartilhada entre repositórios | proposed | harness | Glossário, RNs, constituição e ADRs são fonte única no backend; o front referencia por workspace |
| [ADR-002](002-convencoes-de-git.md) | Convenções de git | proposed | harness | Branch `ab-NNNNN-slug` (sem `#`); worktrees em raiz curta; vínculo com PBI via `AB#NNNNN` no commit/PR |
| [ADR-003](003-agnostico-de-ia-e-framework.md) | Agnóstico de ferramenta de IA e de framework de desenvolvimento | proposed | harness | AGENTS.md é o ponto de entrada canônico; o harness valida o resultado, nunca a ferramenta |
| [ADR-004](004-fronteira-do-artefato-de-framework.md) | Fronteira do artefato de framework de desenvolvimento | proposed | harness | Artefato de kit é scratch efêmero não versionado; o resultado é aterrissado nos docs canônicos |
| [ADR-005](005-plataforma-dotnet10.md) | Plataforma .NET 10 com C# 14 em toda a solution | accepted | plataforma | .NET 10, nullable, primary constructors e file-scoped namespaces obrigatórios em todo projeto |
| [ADR-006](006-deployaveis-api-functions.md) | Dois deployáveis — API e Azure Functions | accepted | plataforma, deploy-ci, assincrono | Tarefas agendadas só em Functions Timer Trigger; API nunca hospeda jobs |
| [ADR-007](007-deploy-branch-appservice.md) | Deploy por branch via GitHub Actions em App Service | accepted | deploy-ci | develop/qa/master deployam por push; publish Release self-contained linux-x64 |
| [ADR-008](008-ci-build-test-gate.md) | Build e testes como gate obrigatório de PR | accepted | deploy-ci, testes | Nenhum merge sem build e dotnet test verdes no CI |
| [ADR-009](009-endpoints-carter.md) | Endpoints exclusivamente via Carter modules | accepted | api | Um {Entidade}Endpoint por prefixo; Controllers MVC e módulos avulsos proibidos |
| [ADR-010](010-grupo-apiv1-auth-default.md) | Grupo api/v1 com autorização por default | accepted | api, seguranca | Toda rota nasce autenticada; AllowAnonymous é opt-out explícito; versão por rota |
| [ADR-011](011-pipeline-request-unico.md) | Pipeline de request único e centralizado | accepted | api | Handler delega ao RequestHandler; try/catch por endpoint proibido |
| [ADR-012](012-http-semantico-problemdetails.md) | Códigos HTTP semânticos com mapa canônico | accepted | api | RFC 9457 + traceId/correlationId; 400/401/403/404/409/422/500 por semântica, nunca por negócio |
| [ADR-013](013-autorizacao-roles-helpers.md) | Autorização por role via helpers e policies fail-closed | accepted | api, seguranca | Só helpers de extensão e policies nomeadas; AuthorizeAttribute inline proibido |
| [ADR-014](014-claims-transformation-identidade.md) | Claims transformation como fonte única de identidade | accepted | api, seguranca | Identidade só das claims enriquecidas com cache; re-parse de JWT proibido |
| [ADR-015](015-jwt-chave-simetrica.md) | Validação do JWT por chave simétrica | accepted | seguranca | Authority OIDC + chave simétrica via Options; rotação manual aceita |
| [ADR-016](016-cors-gateway.md) | CORS aberto com restrição no gateway | accepted | seguranca, api | Política aberta na API; origem restringida pelo gateway (API nunca exposta sem ele) |
| [ADR-017](017-sem-rate-limiting.md) | Sem rate limiting na aplicação | accepted | seguranca, api | Rate limiting só no gateway/WAF; nunca em código de aplicação |
| [ADR-018](018-openapi-scalar.md) | OpenAPI + Scalar em ambientes internos | accepted | api, operacao | Scalar ligado em dev/QA e desligado em produção; rotas declaram contrato |
| [ADR-019](019-cultura-ptbr.md) | Cultura fixa pt-BR no pipeline HTTP | accepted | api | Cultura e mensagens pt-BR fixas; sem negociação de idioma |
| [ADR-020](020-usecase-pattern.md) | UseCase pattern com interface por ação | accepted | application | sealed {Ação}UseCase : I{Ação}UseCase em pasta padrão com Requests/Responses/Validators |
| [ADR-021](021-di-assembly-scanning.md) | Registro de DI por assembly scanning | accepted | application, plataforma | UseCases e validators registrados por convenção; registro manual proibido |
| [ADR-022](022-erros-excecoes-tipadas.md) | Erro de negócio por exceções tipadas | accepted | application, api | NotFound/Conflict/BusinessRule lançadas pelo UseCase, mapeadas 1:1 pro ProblemDetails |
| [ADR-023](023-dados-via-unitofwork.md) | Dados exclusivamente via UnitOfWork/repositórios | accepted | application, dados | DbContext nunca na Application; consultas viram métodos de repositório |
| [ADR-024](024-mapeamento-manual.md) | Mapeamento DTO↔entidade manual | accepted | application | Sem AutoMapper; conversões explícitas por construtor, initializer ou mapper estático |
| [ADR-025](025-usecase-sem-composicao.md) | Composição entre UseCases proibida | accepted | application | UseCase nunca injeta UseCase; lógica compartilhada vira serviço |
| [ADR-026](026-core-sem-dependencias.md) | Core sem dependências externas | accepted | dominio | Domínio só com BCL; contratos de infra como interfaces no Core |
| [ADR-027](027-entidades-ricas.md) | Entidades ricas, sem Specification pattern | accepted | dominio | Setters privados, construção rica, invariantes como métodos; consultas nos repositórios |
| [ADR-028](028-domain-integration-events.md) | Domain Events e Integration Events distintos | accepted | dominio, integracoes | Fatos do negócio no Core; contratos externos fora; tradução explícita entre eles |
| [ADR-029](029-uuidv7.md) | Identidade com chave única UUIDv7 | accepted | dominio, dados | Guid.CreateVersion7() gerado pela aplicação; sem identidade dupla nem IDENTITY |
| [ADR-030](030-auditoria-automatica.md) | Auditoria obrigatória e automática | accepted | dominio, dados | Base com CreatedAt/By preenchida só por SaveChangesInterceptor; sempre UTC |
| [ADR-031](031-enums-string.md) | Enums com prefixo E, persistidos como string | accepted | dominio, dados | HasConversion<string>; nunca int; rótulos de exibição fora do domínio |
| [ADR-032](032-read-models-records.md) | Read-models como records retornados por repositórios | accepted | dominio, dados | Projeção direta pra sealed record; entidade EF nunca atravessa a borda |
| [ADR-033](033-fluent-api-mapeamento.md) | Mapeamento EF exclusivamente por Fluent API | accepted | dados | Um {Entidade}Mapping por entidade; Data Annotations proibidas; precisão explícita |
| [ADR-034](034-fk-restrict.md) | FKs com DeleteBehavior.Restrict global | accepted | dados | Cascade delete nunca; exclusão composta é operação de domínio explícita |
| [ADR-035](035-query-filters-multitenant.md) | Multi-tenant por Global Query Filters | accepted | dados, seguranca | Filtro global por tenant com accessors anuláveis; IgnoreQueryFilters proibido na aplicação |
| [ADR-036](036-repositorio-agregado-uow.md) | Repositório por agregado com DbContext como UoW | accepted | dados | Um repositório por agregado; registro único via DI; SaveChanges só pelo UnitOfWork |
| [ADR-037](037-escrita-change-tracker.md) | Escrita exclusivamente via change tracker | accepted | dados, seguranca | SQL cru de escrita proibido; bulk excepcional é migration Flyway |
| [ADR-038](038-leitura-asnotracking.md) | Leituras com AsNoTracking e projeção direta | accepted | dados | Leitura sem tracking, projetando no banco; paginação sempre no banco |
| [ADR-039](039-mongodb-nao-relacional.md) | MongoDB restrito a dados não relacionais | accepted | dados | Só logs/payloads/auditoria via IMongoRepository; agregados nunca no Mongo |
| [ADR-040](040-cache-idistributedcache.md) | Cache via IDistributedCache | accepted | dados, operacao | Sempre IDistributedCache (inicia em memória); IMemoryCache direto proibido |
| [ADR-041](041-flyway-dono-schema.md) | Flyway único dono do schema em repo dedicado | accepted | migrations, dados | EF Migrations proibidas; aplicação só via CI no fluxo develop→qa→master |
| [ADR-042](042-migration-timestamp.md) | Versionamento de migration por timestamp puro | accepted | migrations | V{yyyyMMddHHmmss}__descricao-kebab; outOfOrder=false em config e CI |
| [ADR-043](043-migrations-forward-only.md) | Migrations imutáveis (forward-only) | accepted | migrations | Migration aplicada nunca é editada; correção é sempre migration nova |
| [ADR-044](044-resiliencia-http-universal.md) | Resiliência HTTP universal | accepted | integracoes | Todo client é Refit + factory + Standard Resilience; HttpClient cru proibido |
| [ADR-045](045-motor-services-providers-acl.md) | Motor em Services, adaptador em Providers com ACL | accepted | integracoes, dominio | Contrato no motor; mappers do provider são ACL; modelo do parceiro nunca vaza |
| [ADR-046](046-base-url-parceiro.md) | Base URL de parceiro com hierarquia única | accepted | integracoes, operacao | Config da engine → override por env var; default hardcoded proibido |
| [ADR-047](047-webhooks-header-secreto.md) | Webhooks autenticados por header secreto | accepted | integracoes, seguranca | Header secreto obrigatório, request logado no Mongo, processamento idempotente |
| [ADR-048](048-email-smtp-mailkit.md) | E-mail via SMTP (MailKit) atrás de IMailService | accepted | integracoes | MailKit/SMTP com Options única; templates vivem fora do projeto de mail |
| [ADR-049](049-documentos-por-referencia.md) | Documentos por referência no parceiro | accepted | integracoes, dados | Sem blob próprio; upload por repasse, download por URL, banco só metadados |
| [ADR-050](050-async-channel-reconciliador.md) | Assíncrono com Channel, reconciliador e idempotência | accepted | assincrono, application | 202 + Channel bounded; estado persistido antes; reconciliador periódico; retry só idempotente |
| [ADR-051](051-progresso-polling.md) | Acompanhamento assíncrono por polling | accepted | assincrono, api | Endpoint GET de status sobre estado persistido; SSE/WebSocket proibidos |
| [ADR-052](052-netarchtest-gate.md) | Testes de arquitetura como gate permanente | accepted | testes, dominio | NetArchTest nos unitários trava camadas; Skip permanente proibido |
| [ADR-053](053-options-pattern.md) | Options Pattern para toda configuração | accepted | operacao, plataforma | Uma classe por seção, BindConfiguration + ValidateOnStart; estáticas só constantes |
| [ADR-054](054-secrets-fora-repo.md) | Secrets fora do repositório | accepted | operacao, seguranca | Nenhum secret versionado; Local.json no dev, env vars nos hosts |
| [ADR-055](055-opentelemetry.md) | OpenTelemetry com correlação ponta a ponta | accepted | operacao | OTel → Azure Monitor; correlationId propagado e exposto; redação de dados sensíveis |
| [ADR-056](056-testes-unitarios-stack.md) | Stack e convenções de teste unitário | accepted | testes | xUnit + NSubstitute + FluentAssertions; espelha src; fakes só em fronteiras; TDD |
| [ADR-057](057-politica-cobertura.md) | Política de cobertura e escopo da suíte | accepted | testes | Cobertura prioriza usecases/domínio; repositórios sem teste; integração adiada com risco registrado |
| [ADR-058](058-artefatos-ingles-docs-ptbr.md) | Nomes de artefatos em inglês; documentação e UI em pt-BR | accepted | dominio, api, plataforma | Artefatos de código em inglês com mapa 1:1 no glossário; docs, UI, mensagens e commits em pt-BR |
| [ADR-059](059-gate-exec-plan-no-harness.md) | Gate de exec-plan no harness | accepted | harness | Diff que toca `src/` exige exec-plan ativo ou dispensa em commit (`Exec-plan:`); força a decisão de triagem, não o artefato |
| [ADR-060](060-catalogo-modalidades-dois-mundos.md) | Catálogo de Modalidades — dois mundos ligados por mapeamento | superseded (ADR-061) | dominio, integracoes | Modalidade curada × Modalidade Importada da fonte, ligadas por mapeamento próprio (substituído pelo ADR-061) |
| [ADR-061](061-modalidade-derivada-da-global-modality.md) | Catálogo de Modalidades — Modalidade derivada da Modalidade Global da OnPoint | accepted | dominio, integracoes | Modalidade = Modalidade Global da OnPoint (import find-or-create por id global) + curadoria manual; sem Grupo no Smart; vínculo Importada→Modalidade direto (id global), override manual preservado; sem tabela de mapeamento; Fila só p/ exceções |
| [ADR-062](062-tags-e-clausulas-no-ciclo-de-catalogo.md) | Tags e Cláusulas Particulares no ciclo de catálogo | accepted | dominio, integracoes | Tag/Cláusulas vêm embutidas no objeto da modalidade (`GetModalityObject`, por modalidade); passo roda no ciclo de catálogo após o upsert (RN-040/041/042); `ImportedModalityTag` 1:1, `ImportedModalityParticularClause` N por (modalidade, id externo); resiliência/preservação herdadas (RN-038/039) |
