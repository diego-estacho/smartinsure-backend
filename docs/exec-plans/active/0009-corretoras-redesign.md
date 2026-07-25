# Exec-plan 0009 — Corretoras: redesign do CRUD (RN-018/019 revisadas, RN-032..RN-034)

Status: em andamento — slug provisório `corretoras-redesign` (AB#/PBI pendente)
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, RNs em `docs/product-specs/regras-de-negocio/corretoras.md` (RN-018..RN-021 + as novas RN-032..RN-034), RN-013..RN-017 (`pessoas.md`, busca/importação de Pessoa reaproveitada), RN-022 (`motor-de-calculo.md`, Habilitação de Seguradora), glossário (Corretora/Broker, situação apresentada derivada, contato da Corretora), `open-decisions.md` (OPEN-03 sem restrição de Perfil; OPEN-04 uso do Birô — gatilho de preview de cadastro decidido nesta atividade). Handoff de design: `../../../prototipos/corretoras/design_handoff_crud_corretoras/README.md`.

## Ratificação (no lugar da PO)

Decisões de domínio abaixo **ratificadas por Diego Estácho no lugar da PO em 2026-07-25** (registrar confirmação formal da PO). Sem elas o redesign não é implementável (regra: nunca inventar RN):

- **Situação apresentada da Corretora** (RN-033) — além de Ativa/Inativa (status armazenado, ratificado 2026-07-17), a listagem e o detalhe passam a mostrar **Incompleta**, um valor **derivado no servidor** (não é status novo, não altera a máquina de estados nem cria transição): Corretora Ativa cujo cadastro não tem nome fantasia **ou** e-mail de contato aparece como Incompleta.
- **Consulta de CNPJ somente leitura** (RN-032) — o cadastro passa a consultar a Receita/Birô **sem gravar nada**; a Corretora só é criada na confirmação (RN-019 revisada). A busca genérica de Pessoa (RN-014, usada por tomadores/consulta de crédito) fica **intocada**.
- **Contato da Corretora** (RN-034 + glossário) — nome fantasia (dado já existente da Pessoa) + e-mail, telefone e responsável (**novos**, no vínculo de papel Corretor) tornam-se dados complementares editáveis.

## Objetivo

Redesenhar a jornada Corretoras de ponta a ponta, com fidelidade ao handoff de design, sem regredir regra de negócio no cliente:

1. **Listagem** com filtro, ordenação e paginação **server-side** (base pode passar de 10 mil): busca (CNPJ/razão social/nome fantasia), abas de situação com contagem (Todas/Ativas/Incompletas/Inativas), filtros avançados (situação, seguradora habilitada, motor de cálculo, setor, período de cadastro), exportar.
2. **Cadastro** que **não persiste antes da confirmação**: consulta CNPJ somente leitura, tratamento de "CNPJ já cadastrado", dados complementares e ativação na confirmação, descarte sem deixar registro.
3. **Detalhe** com Visão geral, Habilitações, Produção e Histórico, alerta de cadastro incompleto, editar/exportar/inativar.
4. **Habilitar seguradora** (reaproveita RN-022) e **edição** de dados complementares.

## Contrato (backend primeiro; front consome os types gerados)

- `GET /api/v1/brokerages` — **revisado (RN-018)**: query `q`, `situation` (`Active|Inactive|Incomplete`), `insurerId`, `calculationEngine`, `sector` (`Public|Private`), `registeredFrom`, `registeredTo`, `page`, `pageSize`. Resposta paginada + `counts` por situação (mesmos filtros exceto a própria situação) para as abas. Item ganha `situation` (derivado), `registeredAt` (data do vínculo Corretor), `enabledInsurers` (nomes + total) e `calculationEngine`.
- `GET /api/v1/brokerages/preview?cnpj=` — **novo (RN-032)**: consulta Birô **somente leitura**, devolve dados da Receita + `alreadyRegistered`/`existingBrokerageId` quando já houver papel Corretor. Nada é gravado.
- `POST /api/v1/brokerages` — **revisado (RN-019)**: body `{ cnpj, socialName?, contactEmail?, contactPhone?, responsibleName?, activateOnSave }`. Cria Pessoa (importando do Birô se nova) + papel Corretor **na confirmação**; `activateOnSave=false` nasce Inativa. Recusa se já houver papel Corretor.
- `PATCH /api/v1/brokerages/{id}` — **novo (RN-034)**: dados complementares (nome fantasia → `Person.SocialName`; contato → vínculo Corretor). Não toca dados da Receita (import-once, RN-014).
- `PATCH /api/v1/brokerages/{id}/status` — inalterado (RN-021).
- Habilitações (`brokerage-insurer-enablements`) — inalteradas (RN-022).
- `GET /api/v1/brokerages/{id}/history` — **novo (RN-035)**: linha do tempo **real** derivada da auditoria (criação da Corretora, cada Habilitação e sua mudança de situação, última edição de complementares), com data/hora e autor. Sem tabela de eventos nova.
- **Produção**: sem fonte de dados nesta fase — os domínios Cotação/Apólice/Sinistro/Prêmio não existem e cotar/emitir é OPEN-07 (dono PO). A aba existe no front com **estado vazio honesto** (sem número falso), não com mock; liga em `GET /brokerages/{id}/production` quando os domínios existirem (TD-006).

## Tarefas

- [ ] RN-018/RN-019 revisadas e RN-032..RN-034 catalogadas em `corretoras.md`; glossário com situação derivada + contato da Corretora; `open-decisions.md` atualizado (OPEN-04 preview de cadastro).
- [ ] Migration no `smartinsure-dbmigration` (branch `develop`): colunas de contato no `PersonRoles` (`ContactEmail`, `ContactPhone`, `ResponsibleName`, nulas — escopo Corretor, como `Status`). Data de cadastro reaproveita `PersonRoles.CreatedAt` (sem coluna nova).
- [ ] Core: `PersonRole` ganha contato (Corretor) + comportamento de edição; regra de situação derivada (Ativa/Incompleta/Inativa) no domínio; sem status novo no enum.
- [ ] Infra.Data: `PersonRoleMapping` 1:1 com a migration; `PersonRepository.ListBrokeragesAsync` com todos os filtros + `counts` + join de habilitações/motor/setor; DTOs `BrokerageListItemDto`/`BrokerageDetailsDto` ampliados.
- [ ] Application: `ListBrokerages` (RN-018 revisada), `PreviewBrokerageByCnpj` (RN-032, somente leitura), `CreateBrokerage` (RN-019 revisada, complementares + ativação), `UpdateBrokerage` (RN-034), `GetBrokerageHistory` (RN-035, timeline da auditoria); validators.
- [ ] Api: rotas `GET /preview`, `PATCH /{id}`, `GET /{id}/history` no `BrokeragesEndpoint`; `GET /` com os query params novos.
- [ ] Testes `[Trait("RuleId", ...)]` — domínio + use cases: situação derivada (Ativa/Incompleta/Inativa), **preview não grava**, criação só na confirmação, edição não toca Receita, filtros combinados e contagem.
- [ ] Contrato `docs/generated/openapi.json` publicado; front regenera types.
- [ ] Frontend (kit primeiro, ADR-022/ADR-013): `SiPageBack` (voltar + breadcrumb reutilizável), `SiMetric` (metric card), ícones novos em `lib/icons.ts`; vitrine `/dev/ui` + de-para atualizados.
- [ ] Frontend telas: listagem (abas+contagem, busca, drawer de filtros, chips, exportar, paginação, skeleton/vazio/erro); cadastro em modal em etapas (sem persistir antes do confirmar, CNPJ já cadastrado, descarte); detalhe (abas Visão geral/Habilitações/Produção/Histórico, alerta incompleto, menu "…", inativar) — **Produção com estado vazio honesto** (sem número falso, TD-006), **Histórico real** (RN-035); habilitar seguradora; mobile (cards, bottom sheet, tela cheia, ações fixas).
- [ ] BFF (`server/api/brokerages/*`) + composables (`useBrokerages` estendido, `useBrokerageHistory` consumindo o endpoint real); mapa de situação com Incompleta. Métricas derivadas de produção na Visão geral/Produção mostram estado vazio honesto até os domínios existirem.
- [ ] Testes front `describe('RN-NNN ...')` + E2E Playwright: filtros combinados, paginação, cadastro-não-persiste-antes-da-confirmação, validação da habilitação; evidência desktop + mobile.
- [ ] PRs: dbmigration (→ develop) antes do backend (→ main), depois frontend — mesmo vínculo (AB# pendente — slug `corretoras-redesign`); `git pull origin main` antes de cada push.

## Critérios de aceite

- `dotnet build` e `dotnet test` verdes; `python scripts/check-harness.py` verde nos repos tocados; lint + typecheck + unit + E2E verdes no front.
- Consulta de CNPJ no cadastro **não grava nada** (teste de use case comprova ausência de persistência); Corretora só existe após o `POST` de confirmação; cancelar no meio não deixa registro (RN-032/RN-019).
- Situação Incompleta é derivada no servidor e coerente na aba, na contagem, no filtro e no detalhe; nenhuma regra de situação roda no cliente (RN-033).
- Filtro, ordenação e paginação são server-side; qualquer alteração de filtro volta à página 1; contagem das abas bate com os filtros aplicados (RN-018).
- Edição altera só dados complementares; dados da Receita seguem não editáveis (RN-034/RN-014).
- Habilitação exige seguradora, motor, base URL e key; validação e mensagens por campo (RN-022).

## Evidências

**Backend (2026-07-25):** `dotnet build` sem erros; `dotnet test` — **310/310 aprovados** (novos `[Trait("RuleId")]` RN-032/033/034/035 + revisão RN-018/019), inclui o gate de arquitetura NetArchTest; `python scripts/check-harness.py` → `harness ok`. Comportamentos cobertos: preview **não grava** (RN-032), criação **só na confirmação** com complementares + ativação (RN-019), situação **derivada** Ativa/Incompleta/Inativa (RN-033), filtros/contagem server-side (RN-018), edição **não toca a Receita** (RN-034), histórico da auditoria (RN-035). Migration `V20260725120000__adicionar-contato-corretor-em-person-roles.sql` na `develop` (dbmigration).

**Pendente (fase front):** regenerar `docs/generated/openapi.json` com o app rodando e os types no front; telas do front + testes unit + E2E Playwright (screenshots desktop + mobile); aplicar a migration no banco de dev; ratificação formal da PO; AB#/PBI.
