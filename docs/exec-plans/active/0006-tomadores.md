# Exec-plan 0006 — Tomadores: cadastro, endereços e Nomeação (RN-025..RN-028)

Status: em andamento — iniciado em 2026-07-20
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, RNs em `docs/product-specs/regras-de-negocio/tomadores.md`, RN-013..RN-017 (`pessoas.md`, fluxo de busca/importação reaproveitado), glossário (termo Nomeação de Tomador `PolicyHolderAppointment` — proposto 2026-07-20, aguardando ratificação da PO), OPEN-03 (sem restrição de Perfil nesta fase).

## Objetivo

Jornada de Tomadores: listagem e detalhes (RN-025) reaproveitando o cadastro de Pessoas (criação = busca de Pessoa no contexto tomador + vínculo de papel), endereços adicionais com principal fixo do Birô (RN-026) e Nomeação de Tomador — vínculo Corretora×Tomador×Seguradora independente da Habilitação, com no máximo uma Vigente por par Tomador×Seguradora, substituição direta e encerramento avulso com histórico (RN-027/RN-028).

## Tarefas

- [x] RN-025..RN-028 catalogadas em `tomadores.md`; glossário com termo e status da Nomeação (propostos, aguardando ratificação da PO).
- [x] Migration no `smartinsure-dbmigration`: tabela `PolicyHolderAppointments` (FKs `PolicyHolderId → Persons`, `BrokerageId → Persons`, `InsurerId → Insurers`; `Status` string Vigente/Encerrada por nome estável `Active`/`Ended`; `StartedAt`/`EndedAt`; unique filtrado `(PolicyHolderId, InsurerId) WHERE Status='Active'`).
- [x] Core: entidade `PolicyHolderAppointment` (rica, nasce `Active`, `End()` marca `Ended` + `EndedAt`), `EPolicyHolderAppointmentStatus`, `IPolicyHolderAppointmentRepository`; comportamento de endereço adicional em `Person` (`AddAdditionalAddress`/`UpdateAdditionalAddress`/`RemoveAdditionalAddress` — principal intocável, RN-026).
- [x] Application: use cases ListPolicyHolders/GetPolicyHolder (RN-025, detalhe com endereços e nomeações), CreatePolicyHolder (reusa busca/importação de Pessoa no contexto tomador — RN-013/014/016/017), Add/Update/RemoveAdditionalAddress (RN-026), CreateAppointment (RN-027 — Corretora e Seguradora Ativas; substituição na mesma operação encerra a Vigente — RN-028), EndAppointment (RN-028).
- [x] Infra.Data: mapping 1:1 com a migration (índice unique filtrado), repository, DI.
- [x] Api: `PolicyHoldersEndpoint` (list/get/create, endereços, nomeações) — autenticado, sem restrição de Perfil (OPEN-03).
- [x] Testes `[Trait("RuleId", "RN-025".."RN-028")]` — domínio + use cases; substituição pela mesma Corretora recusada; Encerrada nunca volta a Vigente.
- [x] Validar migration no banco de dev (VPS, aplicada via sqlcmd — `SET QUOTED_IDENTIFIER ON` exigido pelo índice filtrado; baseline Flyway na VPS segue pendente).
- [x] Contrato `openapi.json` publicado (6 rotas `policy-holders`).
- [x] Frontend: páginas `/tomadores` (lista), criação por CNPJ (busca de Pessoa), detalhe com endereços (CRUD de adicionais) e nomeações (criar/substituir/encerrar + histórico); BFF + composables + mapa de status Vigente/Encerrada; types regenerados.
- [x] Testes front `describe('RN-NNN ...')` + E2E Playwright da jornada.
- [ ] PRs: dbmigration (→ develop) antes do backend (→ main), depois frontend — mesmo vínculo (AB# pendente — slug provisório `tomadores`).

## Critérios de aceite

- `dotnet build` e `dotnet test` verdes; `python scripts/check-harness.py` verde nos repos tocados.
- Máximo uma Nomeação Vigente por par Tomador×Seguradora (constraint + use case); criação exige Corretora e Seguradora Ativas (RN-027).
- Substituição encerra a Vigente e cria a nova na mesma operação; substituição pela mesma Corretora recusada; encerramento avulso deixa o par sem Vigente; histórico preservado com período (RN-028).
- Endereço principal não editável/removível; adicionais com CRUD livre; sempre exatamente um principal (RN-026).
- Listagem devolve apenas Pessoas com papel tomador; dados do Birô não editáveis nas telas (RN-025).

## Evidências

- Backend: `dotnet build` sem erros; `dotnet test` — 246/246 aprovados (24 novos com `[Trait("RuleId")]` RN-025..RN-028); `check-harness.py` → `harness ok`.
- Migration: `V20260720125347__criar-tabela-policy-holder-appointments.sql` aplicada no banco de dev (VPS) — tabela + `UX_PolicyHolderAppointments_ActivePair` (unique filtrado `Status='Active'`) confirmados via `sys.indexes`.
- Contrato: `docs/generated/openapi.json` com `/api/v1/policy-holders` (GET/POST), `/{id}` (GET), `/{id}/addresses` (POST/PUT/DELETE), `/{id}/appointments` (POST) e `/{id}/appointments/{appointmentId}/end` (PATCH).
- Frontend: lint e typecheck sem erros; unit 83/83; E2E Playwright 13/13 (7 da jornada de tomadores); `check-harness.py` → `harness ok`. Telas padronizadas com o padrão canônico de Corretoras (hero com ação primária, painéis extraídos, dialogs e tokens).
- Pendências: ratificação da PO (termo Nomeação de Tomador e RN-025..028), AB#/PBI, PRs, migration Flyway na VPS (baseline pendente — aplicada via sqlcmd).
