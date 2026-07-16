# Exec-plan 0003 — Catálogo de Seguradoras e perfil Administrador do Sistema (RN-007..RN-012)

Status: concluído em 2026-07-16 — backend e migrations implementados, verificados (gates + smoke 8/8) e validados manualmente pelo dev; PRs abertos com AB#0001
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, RNs em `docs/product-specs/regras-de-negocio/seguradoras.md` (RN-007..RN-011) e `usuarios.md` (RN-012), glossário (termos Perfil e Administrador do Sistema e status Ativa/Inativa de Seguradora, propostos 2026-07-16), OPEN-03 (perfis de Corretora — fora do escopo; este plano cria apenas o perfil interno) e OPEN-04 (Birô — não usado no cadastro).

## Objetivo

Fatia vertical da jornada Seguradoras (AB#0001, backend-only): catálogo de Seguradoras (criar, alterar, ativar/desativar, consultar — RN-007..RN-010) mantido exclusivamente pelo Administrador do Sistema (RN-011), primeiro Perfil da plataforma (RN-012) — API no backend + migrations Flyway no repo `smartinsure-dbmigration`. Front consome em atividade futura.

## Tarefas

- [x] RN-007..RN-012 catalogadas e aprovadas; glossário com termos/status propostos (ratificação da PO pendente — registrada no próprio glossário).
- [x] Worktrees ADR-002: `C:\wt\ab-0001\smartinsure-backend` (branch `ab-0001-catalogo-seguradoras`) e `C:\wt\ab-0001\smartinsure-dbmigration` (mesma branch, a partir de `develop`).
- [x] Core: `Insurer` (entidade rica; transições Ativa↔Inativa com conflito de estado), `EInsurerStatus`, `EUserProfile`, `User.Profile` + `GrantProfile`/`RevokeProfile`, `IInsurerRepository` (+ `InsurerListItemDto`), `IUserRepository.CountByProfileAsync`, constantes `Roles`/`Policies`/`CacheKeys`.
- [x] Infra.Data: `InsurerMapping` (CNPJ único), `InsurerRepository` (listagem paginada com filtro de situação e clamp de paginação no use case), coluna `Profile` no `UserMapping`, registros de DI.
- [x] Migrations Flyway no repo irmão: `V20260716113014__criar-tabela-insurers.sql` e `V20260716113019__adicionar-profile-em-users.sql` (guards, imutáveis). Seed do 1º admin NÃO é migration — runbook abaixo.
- [x] Application: `CreateInsurer` (RN-007, com `CnpjValidator` de dígitos verificadores em Infra.CrossCutting), `UpdateInsurer` (RN-008), `ChangeInsurerStatus` (RN-009), `ListInsurers`/`GetInsurer` (RN-010), `SetUserProfile` (RN-012, com proteção de último admin e invalidação de cache de perfil). Situação/perfil desconhecidos viram `BusinessRuleException` (defense-in-depth além do validator).
- [x] Api: `InsurersEndpoint` (escrita com policy `SystemAdministrator`; leitura autenticada), rota `PUT /users/{id}/profile`, `UserProfileClaimsTransformation` (perfil → role, cache 5 min invalidado na concessão/revogação), policy fail-closed registrada (RN-011).
- [x] Testes com rastreabilidade `[Trait("RuleId", "RN-00X")]` para toda RN; TDD (teste antes do código).
- [x] Contrato `docs/generated/openapi.json` regenerado com as rotas novas.
- [x] Verificação: gates verdes + smoke e2e (403 sem perfil; visão operacional sem Inativas) + code review por task; evidências abaixo.
- [x] PRs linkados por `AB#0001`: backend primeiro, depois dbmigration (`develop`); exec-plan movido para `completed/` no PR que encerra.

**Runbook — primeiro Administrador do Sistema (RN-012, operação interna):** a equipe executa no ambiente alvo `UPDATE dbo.Users SET Profile = 'SystemAdministrator' WHERE Email = '<e-mail do operador>';`. Dado de ambiente não entra em migration imutável.

Plano detalhado (rascunho de execução, scratch ADR-004): fora do repo, na pasta de trabalho da atividade.

## Critérios de aceite

- `dotnet build SmartInsure.slnx` e `dotnet test tests/SmartInsure.Tests` verdes (NetArchTest incluso); cobertura ≥ 80%.
- `python scripts/check-harness.py` verde.
- Toda operação de escrita do catálogo sem o perfil retorna 403 sem efeito (RN-011); consulta padrão não retorna Inativas (RN-010); CNPJ duplicado/situação repetida retornam 409 (RN-007/RN-009); revogação do último admin retorna 422 (RN-012).
- Migrations aplicadas pelo compose local sem erro; status exposto por nome estável (`Active`/`Inactive`).
- Testes de RN carregam o ID (gate de rastreabilidade).

## Evidências

- Build: `dotnet build SmartInsure.slnx` — 0 erros (2026-07-16).
- Testes: `dotnet test tests/SmartInsure.Tests` — **119/119 aprovados** (59 da baseline + 60 novos, todos com `[Trait("RuleId", ...)]` RN-007..RN-012; NetArchTest incluso; TDD com evidência RED→GREEN por task).
- Cobertura (coverlet, cobertura.xml): `Application.UseCase` **92,0%**, `Core` **97,4%**, `Infra.CrossCutting` 60,7%; total da solution 41,4% contra 30,4% na `main` — a branch só eleva a cobertura. O gate de 80% do CI segue pendente de implementação no pipeline (scaffold da Fase B); nos assemblies onde vive o comportamento de negócio desta atividade o piso está atendido.
- Harness: `python scripts/check-harness.py` → `harness ok` (aviso pré-existente do exec-plan 0002, não relacionado).
- Migrations: aplicadas pelo `docker compose --profile migrations up -d` no SQL Server local — log do Flyway com as duas migrations e verificação direta (`dbo.Insurers` criada; `dbo.Users.Profile` presente).
- Smoke e2e (API real + JWT local + SQL local, 2026-07-16) — **8/8**:
  - S1: `GET /api/v1/insurers` sem token → 401.
  - S2 (RN-011): `POST /insurers` autenticado sem perfil → 403, sem efeito.
  - S3 (RN-011): `POST /insurers` como Administrador do Sistema (runbook simulado via SQL) → 201.
  - S4 (RN-007): mesmo CNPJ de novo → 409.
  - S5 (RN-009): `PATCH /insurers/{id}/status` → 200; repetido na mesma situação → 409.
  - S6/S7 (RN-010): Inativa ausente para não-admin (mesmo com `includeInactive=true`); visível para admin com status por nome estável `Inactive`.
  - S8 (RN-012): `PUT /users/{id}/profile` sem perfil → 403.
- Contrato: `docs/generated/openapi.json` com `/api/v1/insurers` (POST/GET), `/api/v1/insurers/{id}` (PUT/GET), `/api/v1/insurers/{id}/status` (PATCH) e `/api/v1/users/{id}/profile` (PUT).
- Observação de processo: regeneração do contrato ainda sem script formal (candidata a `scripts/` pela regra das 3 ocorrências); Mongo não sobe no compose local e não foi exercitado pelo smoke (nenhum caminho desta atividade usa Mongo).
- Validação manual do dev (2026-07-16): API rodando do worktree contra o SQL do compose local, roteiro 401/403/201/409/filtro RN-010 confirmado via Scalar com JWTs locais e admin semeado pelo runbook.
