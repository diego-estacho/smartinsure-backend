# Exec-plan 0011 — Tags e Cláusulas Particulares (RN-040, RN-041, RN-042) — AB#0004

Status: **concluído** (2026-07-24). Teste ao vivo PlugV2 dev sem defeitos. RN-040/041/042 e os termos de glossário (Tag, Cláusula particular) ratificados pela PO em 2026-07-23 (pré-requisito atendido antes da implementação de comportamento de negócio).

Contexto obrigatório (ler antes de executar): `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/adr/062-tags-e-clausulas-no-ciclo-de-catalogo.md`, `docs/adr/061-modalidade-derivada-da-global-modality.md`, `docs/adr/041-flyway-dono-do-schema.md` (migrations no repo Flyway), RNs em `docs/product-specs/regras-de-negocio/tags-e-clausulas.md` (RN-040/041/042) e `modalidades.md` (RN-034/038/039 reusadas), PRD `specs/004-tags/spec-tags.md`, `open-decisions.md` (OPEN-10 cadência). Base: importação de catálogo (exec-plan `completed/0008` + retrabalho `active/0010`), `ModalityImporter`, `PlugV2CalculationEngine`, `PlugV2ModalityImportClient` + ACL.

## Objetivo

Manter, no produto, uma cópia local sincronizada da **Tag** (objeto/`JsonTag`) e das **Cláusulas particulares** de cada Modalidade Importada Ativa, importadas da OnPoint (`POST /GetModalityObject`, por modalidade) **dentro do ciclo de importação de catálogo** (RN-034), logo após o upsert das Modalidades Importadas. Upsert idempotente (Tag 1:1; Cláusula por chave modalidade+id externo), resiliência por modalidade e reconciliação por inativação (RN-040/041/042; herda RN-038/039). Sem front (renderização do formulário fora de escopo). Cadência configurável+default (prod 05:00 / 30min demais, OPEN-10).

## Contrato PlugV2 (a confirmar por probe dev, no padrão do ADR-045)

`POST {baseUrl}/GetModalityObject` com `{ BrokerCnpj, ModalityUniqueId }` e header `application-key-v2` (por Habilitação). Resposta (envelope `BaseResponse`): `hasError`/`errors` (resiliência, RN-042), `response.object.{jsonTag,text}` (a Tag, RN-040), `response.particularClauses[].{id,name,text,jsonTag}` (cláusulas, RN-041). A ACL traduz o payload observado para o contrato do motor; envelope de erro/transporte sobe como exceção isolada por modalidade.

## Tarefas

- [ ] **T1 — Migration (dbmigration).** `V{ts}__criar-tabelas-tags-e-clausulas.sql` no repo `smartinsure-dbmigration` (branch `ab-0004-tags-e-clausulas`): `dbo.ImportedModalityTags` (FK e **único** por `ImportedModalityId`; `JsonTag`, `ObjectText`, `Status`; auditoria) e `dbo.ImportedModalityParticularClauses` (FK `ImportedModalityId`; `ExternalId`; **único** por `(ImportedModalityId, ExternalId)`; `Name`, `ClauseText`, `JsonTag`, `Status`; auditoria). Guards de existência, forward-only (ADR-041/042/043). Aplicada no docker.
- [ ] **T2 — Core: enums e entidades.** `EImportedModalityTagStatus` e `EImportedModalityClauseStatus` (Active/Inactive, por nome estável); `ImportedModalityTag` (`Create`/`UpdateFromSource` — só com jsonTag; `Deactivate`/`Reactivate`; sempre nasce Ativa, RN-040) e `ImportedModalityParticularClause` (`Create`/`UpdateFromSource` por chave externa; `Deactivate`; reativa ao reaparecer, RN-041). Testes de entidade `[Trait("RuleId","RN-040/041")]`.
- [ ] **T3 — Core: contratos de repositório e do motor.** `IImportedModalityTagRepository` (`GetByImportedModalityAsync`) e `IImportedModalityParticularClauseRepository` (`ListByImportedModalityAsync`); `ModalityObjectResult` (record: `HasError`, `JsonTag?`, `ObjectText?`, `Clauses[]`) e `GetModalityObjectAsync(connectionParameters, brokerCnpj, modalityUniqueId, ct)` em `ICalculationEngine`.
- [ ] **T4 — Infra.Data.** Mappings EF (`ImportedModalityTagMapping` único por `ImportedModalityId`; `ImportedModalityParticularClauseMapping` único por `(ImportedModalityId, ExternalId)`), repositórios, `DbSet`s, DI.
- [ ] **T5 — Integration: PlugV2.** `PlugV2ModalityObjectClient` (HttpClient nomeado, base URL/key por Habilitação, ADR-044) + `PlugV2ModalityObjectAclMapper.Map(raw)` (payload → `ModalityObjectResult`, envelope de erro isolado); `PlugV2CalculationEngine.GetModalityObjectAsync` resolve o client sob demanda. Teste de ACL com amostra do contrato real (ADR-045).
- [ ] **T6 — Application: passo no ModalityImporter.** Após o upsert das modalidades de uma Seguradora bem-sucedida, para cada `ImportedModality` Ativa: chama `GetModalityObjectAsync` (ModalityUniqueId=SourceId); upsert da Tag (RN-040) e das Cláusulas (RN-041); reconciliação — inativa Tag/Cláusulas ausentes numa consulta bem-sucedida (RN-042); falha por modalidade isolada e registrada no `ModalityImportSummary` (RN-042). Testes `[Trait("RuleId","RN-040/041/042")]` contra motor fake (upsert tag, sem jsonTag não sobrescreve, upsert/dedup cláusula, lista vazia inativa, falha isolada não desativa, reconciliação desativa o que sumiu).
- [ ] **T7 — Cadência configurável (OPEN-10).** Ajustar o timer/Options da importação para cadência configurável com default por ambiente (prod cron 05:00 diária; demais 30min), sem valor fixo no código (Options Pattern, ADR-053). Confirmar que o passo de tags entra no disparo sob demanda existente (`POST /modality-imports/run`, sem novo endpoint).
- [ ] **T8 — Gates + contrato.** `dotnet build SmartInsure.slnx`, `dotnet test tests/SmartInsure.Tests` (cobertura ≥80%), `python scripts/check-harness.py`; regenerar `docs/generated/openapi.json` se a superfície da API mudar (não deve — reuso do endpoint).
- [ ] **T9 — Teste ao vivo (dev)** contra corretora real no PlugV2 dev (padrão do exec-plan 0008): importar objeto de modalidades reais, verificar Tag e Cláusulas gravadas/atualizadas/inativadas no banco docker. Segredo só no scratchpad/banco docker (SECURITY.md).

## Critérios de aceite

- **CA-01** (RN-040) — Após um ciclo bem-sucedido, toda Modalidade Importada Ativa com `jsonTag` na origem tem localmente a Tag vigente, 1:1, Ativa; reimportar não duplica.
- **CA-02** (RN-042) — Modalidade que deixou de retornar Tag/Cláusula (ou ficou Inativa) tem a Tag/Cláusula **inativada** após o próximo ciclo bem-sucedido; nada é apagado.
- **CA-03** (RN-042) — Falha na consulta do objeto de uma modalidade não desativa sua Tag/Cláusulas nem interrompe as demais; registrada no sumário.
- **CA-04** (RN-040) — Objeto sem `jsonTag` não cria Tag nem sobrescreve a existente com vazio.
- **CA-05** (RN-041) — Cláusulas do mesmo payload são criadas/atualizadas sem duplicidade pela chave (modalidade, id externo); lista vazia inativa as locais.
- **CA-06** — Em produção a Tag atualiza ≥1x/dia sem intervenção (cadência default 05:00) e é disparável sob demanda pelo Administrador do Sistema (`/modality-imports/run`).
- Gates verdes (build, testes ≥80%, `harness ok`); migration aplicada; teste ao vivo evidenciado.

## Evidências

Implementação concluída em 2026-07-23 por execução subagent-driven (superpowers-feature), com review por tarefa e review whole-branch final. Gates verdes; **teste ao vivo contra PlugV2 dev (corretora Bravo) concluído em 2026-07-24 — sem defeitos**.

- **Backend**: `dotnet build SmartInsure.slnx` → **0 erros**. `dotnet test tests/SmartInsure.Tests` → **362/362** (0 falhas), incluindo NetArchTest. Novos testes carregam `RN-040/041/042`: entidades (4), ACL PlugV2 do objeto (3), passo do importer (9: cria/atualiza tag, sem-jsonTag não sobrescreve e inativa, upsert/dedup cláusula, id duplicado first-wins, lista vazia inativa, falha por HasError e por exceção não desativa, modalidade inativa → inativa tag/cláusulas).
- **`check-harness.py`** → `harness ok` (RNs, IDs únicos, glossário e links validados; único aviso é pré-existente do exec-plan 0002).
- **Migration Flyway**: `smartinsure-dbmigration` `V20260723191118__criar-tabelas-tags-e-clausulas.sql` (commit `db7c271`, branch `ab-0004-tags-e-clausulas`), com índices únicos (`ImportedModalityId`; `(ImportedModalityId, ExternalId)`). **Aplicação local não confirmada** por colisão de volume Docker compartilhado entre worktrees (AB#0003) — não é defeito da migration; o CI aplica.
- **Review final (whole-branch, opus)**: aprovado após correções. "Migration ausente" foi **falso positivo** (a migration está na branch do dbmigration). Corrigidos: env `ModalityImport__Schedule` no `docker-compose` (agendado no stack dev), teste do ramo de inativação por modalidade inativa (RN-042), e ACL passa a descartar cláusula só quando o id é ausente (mantém `id==0`).
- **Contrato/OpenAPI**: superfície da API **não mudou** (reuso de `POST /modality-imports/run`); regeneração do `openapi.json` não necessária (e bloqueada localmente sem segredos de dev).
- **Commits backend** (branch `ab-0004-tags-e-clausulas`): `54d3bc8` entidades · `e1af6ac` persistência · `b7ed5c3` motor+ACL · `7566387` passo no importer · `8ddab3b` testes extras · `92e94f0` cadência · `068d839` fixes do review + docs canônicos.

- **Teste ao vivo (PlugV2 dev, corretora Bravo `34060267000196`, 2026-07-24)** — stack isolado (banco limpo, 18 migrations Flyway aplicadas incluindo `V20260723191118`), Essor + Excelsior habilitadas com a PlugKey de dev. Base URL real dev: `http://gateway.onpoint.com.br/dev/garantia/plugv2` (nos ConnectionParameters da Habilitação — nenhum path fixo no código).
  - **Contrato real (engine + ACL)**: `GetGroupAndModalities` → 74 modalidades (Essor 49, Excelsior 25). `GetModalityObject ×74` → 59 com Tag, 15 sem Tag, 0 erro; **139 Cláusulas** em 25 modalidades; **0 cláusula sem id**; **0 JsonTag inválido** (todos JSON parseável). A ACL traduz o payload real (PascalCase) corretamente.
  - **CA-01/CA-04 (persistência + 1:1)**: banco com 74 Modalidades Importadas, **59 Tags** (todas Ativas, 0 `ImportedModalityId` duplicado), **139 Cláusulas** (0 duplicado por (modalidade, id)). As 15 modalidades sem jsonTag corretamente **não** criaram Tag (RN-040/CA-04).
  - **Idempotência (CA-01)**: reimportação → contagens idênticas (74/59/139), sem duplicidade.
  - **CA-02/CA-03/RN-042 (reconciliação, ao vivo)**: cláusula e tag "fake" injetadas → após reimport bem-sucedida ficaram **Inativas**; cláusulas/tags reais permaneceram Ativas (sem dano colateral). Falha por modalidade não desativa nada (coberto em unit + isolamento confirmado no run).

### Pendências (para infra / deploy)
- **Cadência agendada — infra**: o host isolado do Functions resolve `%ModalityImport:Schedule%` por Application Setting/variável de ambiente, **não** pelo `appsettings.json` do worker. Definir `ModalityImport:Schedule` = `0 0 5 * * *` em produção (default dev/QA `0 */30 * * * *` já no `appsettings.json` e no `docker-compose`). Ver OPEN-10.
- Ratificação formal da PO das RN-040/041/042 e termos de glossário (registrada como ratificada em 2026-07-23).
