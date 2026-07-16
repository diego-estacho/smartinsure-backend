# Exec-plan 0003 — Catálogo de Seguradoras e perfil Administrador do Sistema (RN-005..RN-010)

Status: planejado — RNs aprovadas; implementação não iniciada
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, RNs em `docs/product-specs/regras-de-negocio/seguradoras.md` (RN-005..RN-009) e `usuarios.md` (RN-010), glossário (termos Perfil e Administrador do Sistema e status Ativa/Inativa de Seguradora, propostos 2026-07-16), OPEN-03 (perfis de Corretora — fora do escopo; este plano cria apenas o perfil interno) e OPEN-04 (Birô — não usado no cadastro).

## Objetivo

Fatia vertical da jornada Seguradoras (AB#0001, backend-only): catálogo de Seguradoras (criar, alterar, ativar/desativar, consultar — RN-005..RN-008) mantido exclusivamente pelo Administrador do Sistema (RN-009), primeiro Perfil da plataforma (RN-010) — API no backend + migrations Flyway no repo `smartinsure-dbmigration`. Front consome em atividade futura.

## Tarefas

- [ ] RN-005..RN-010 catalogadas e aprovadas; glossário com termos/status propostos (ratificação da PO pendente — registrada no próprio glossário).
- [ ] Worktrees ADR-002: `C:\wt\ab-0001\smartinsure-backend` (branch `ab-0001-catalogo-seguradoras`) e `C:\wt\ab-0001\smartinsure-dbmigration` (mesma branch, a partir de `develop`).
- [ ] Core: `Insurer` (entidade rica; transições Ativa↔Inativa com conflito de estado), `EInsurerStatus`, `EUserProfile`, `User.Profile` + `GrantProfile`/`RevokeProfile`, `IInsurerRepository` (+ `InsurerListItemDto`), `IUserRepository.CountByProfileAsync`, constantes `Roles`/`Policies`/`CacheKeys`.
- [ ] Infra.Data: `InsurerMapping` (CNPJ único), `InsurerRepository` (listagem paginada com filtro de situação), coluna `Profile` no `UserMapping`, registros de DI.
- [ ] Migrations Flyway no repo irmão: `criar-tabela-insurers` e `adicionar-profile-em-users` (guards, imutáveis; timestamps de criação). Seed do 1º admin NÃO é migration — runbook abaixo.
- [ ] Application: `CreateInsurer` (RN-005, com `CnpjValidator` de dígitos verificadores em Infra.CrossCutting), `UpdateInsurer` (RN-006), `ChangeInsurerStatus` (RN-007), `ListInsurers`/`GetInsurer` (RN-008), `SetUserProfile` (RN-010, com proteção de último admin e invalidação de cache de perfil).
- [ ] Api: `InsurersEndpoint` (escrita com policy `SystemAdministrator`; leitura autenticada), rota `PUT /users/{id}/profile`, `UserProfileClaimsTransformation` (perfil → role, cache 5 min invalidado na concessão/revogação), policy fail-closed registrada (RN-009).
- [ ] Testes com rastreabilidade `[Trait("RuleId", "RN-00X")]` para toda RN; TDD (teste antes do código).
- [ ] Contrato `docs/generated/openapi.json` regenerado com as rotas novas.
- [ ] Verificação: gates verdes + smoke manual (403 sem perfil; visão operacional sem Inativas) + code review; evidências abaixo.
- [ ] PRs linkados por `AB#0001`: backend primeiro, depois dbmigration (`develop`); exec-plan movido para `completed/` no PR que encerra.

**Runbook — primeiro Administrador do Sistema (RN-010, operação interna):** a equipe executa no ambiente alvo `UPDATE dbo.Users SET Profile = 'SystemAdministrator' WHERE Email = '<e-mail do operador>';`. Dado de ambiente não entra em migration imutável.

Plano detalhado (rascunho de execução, scratch ADR-004): fora do repo, na pasta de trabalho da atividade.

## Critérios de aceite

- `dotnet build SmartInsure.slnx` e `dotnet test tests/SmartInsure.Tests` verdes (NetArchTest incluso); cobertura ≥ 80%.
- `python scripts/check-harness.py` verde.
- Toda operação de escrita do catálogo sem o perfil retorna 403 sem efeito (RN-009); consulta padrão não retorna Inativas (RN-008); CNPJ duplicado/situação repetida retornam 409 (RN-005/RN-007); revogação do último admin retorna 422 (RN-010).
- Migrations aplicadas pelo compose local sem erro; status exposto por nome estável (`Active`/`Inactive`).
- Testes de RN carregam o ID (gate de rastreabilidade).

## Evidências

Pendente — preenchido na conclusão de cada gate (saída do `dotnet test`, `check-harness`, log do Flyway e smoke manual da policy).
