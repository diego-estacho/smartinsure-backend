# Exec-plan 0008 — Fundação do modelo de Perfil (RN-032, RN-033, refatoração da RN-012)

Status: em andamento — fatia 0 da jornada Perfis e Permissões (slug provisório `perfis-permissoes`, AB# pendente)
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, RNs em `docs/product-specs/regras-de-negocio/perfis-e-permissoes.md` (RN-032, RN-033) e `usuarios.md` (RN-012), glossário (termos `Profile`/`Permission`/`ProfileScope` — propostos 2026-07-23, ratificados pela PO), OPEN-03/OPEN-09.

## Objetivo

Primeira fatia da jornada Perfis e Permissões: transformar **Perfil de enum em entidade**. Hoje o Perfil é o enum `EUserProfile` (só `SystemAdministrator`) numa coluna `Users.Profile`. Esta fatia cria as entidades `Profile`, `Permission` e `ProfilePermission` (com `ProfileScope`: System/Brokerage/PolicyHolder), migra o `SystemAdministrator` para o novo modelo e **remove o enum no mesmo PR** (AGENTS: dois padrões não convivem), mantendo os critérios da RN-012 (conceder/revogar, guarda de último Administrador do Sistema, recusa por não-admin) e o contrato do endpoint inalterado (front intocado). Os vínculos por Corretora/Tomador (`BrokerageId`/`PolicyHolderId` nascem nullable, sem FK) ficam para a fatia 1.

## Escopo e não-escopo (fatia 0)

- **No escopo:** entidades `Profile`/`Permission`/`ProfilePermission` + `ProfileScope`; migrations das tabelas; seed do Perfil fixo `SystemAdministrator` (dado de catálogo, precedente `LegalNatures`); migração de `Users.Profile` → `Users.ProfileId` (backfill + drop da coluna antiga); refatoração de `SetUserProfileUseCase`/validator/claims para a entidade; remoção de `EUserProfile`.
- **Fora do escopo (fatias seguintes):** catálogo de códigos de Permissão e **enforcement por permissão** (RN-033 completa) — as tabelas nascem, mas nenhuma feature declara/consome permissão ainda; autorização segue por role derivada do nome do Perfil. Criação de Perfil customizado, vínculos Usuário↔Corretora/Tomador, convite, gestão. Perfis-base Corretor/Tomador dependem da OPEN-09.

## Tarefas

- [ ] Migrations no `smartinsure-dbmigration` (ordem imutável): `Permissions`, `Profiles`, `ProfilePermissions`; migração `Users.Profile` → `ProfileId` com seed do Perfil `SystemAdministrator` (GUID estável), backfill e drop da coluna `Profile`.
- [ ] Core: `ProfileScope`; entidades `Profile` (rica: `Create`, `AddPermission`, `HasPermission`), `Permission`, `ProfilePermission`; `User` passa a ter `ProfileId` + navegação `Profile` (sem `EUserProfile`); constante `ProfileNames.SystemAdministrator`.
- [ ] Abstractions: `IProfileRepository` (`GetByNameAsync`, `GetSystemAdministratorAsync`), `IPermissionRepository`; `IUserRepository.CountByProfileAsync` → `CountByProfileIdAsync(Guid)`.
- [ ] Infra.Data: mappings 1:1 (`ProfileMapping`, `PermissionMapping`, `ProfilePermissionMapping`, `UserMapping` ajustado); `ProfileRepository`/`PermissionRepository`; `SmartInsureDbContext` DbSets; DI.
- [ ] Application: `SetUserProfileUseCase` resolve o Perfil por nome e a guarda de último admin por `ProfileId`; `SetUserProfileValidator` sem `Enum.TryParse`.
- [ ] Api: `UserProfileClaimsTransformation` lê `Profile.Name` (constante), não o enum.
- [ ] Remover `EUserProfile.cs` — zero referências no codebase.
- [ ] Testes `[Trait("RuleId", "RN-012")]` (grant/revoke/guarda sobre a entidade) e `[Trait("RuleId", "RN-032")]` (modelo Profile: Create, escopo, permissões); `Permission`/mapeamentos cobertos sem RuleId (RN-033 completa é fatia de enforcement).
- [ ] `dotnet build` + `dotnet test` verdes; `check-harness.py` verde; migration validada no banco de dev.
- [ ] PR: dbmigration (→ develop) antes do backend (→ main), mesmo vínculo (AB# pendente).

## Critérios de aceite

- `Profile`/`Permission`/`ProfilePermission` compilam e mapeiam 1:1 com as migrations; toda FK Restrict; enum como string.
- `Users.ProfileId` (nullable) referencia `Profiles`; coluna `Users.Profile` removida; `EUserProfile` deletado — zero referências.
- RN-012 preservada: conceder o Perfil que já tem é conflito; revogar de quem não tem é conflito; revogação que deixaria a plataforma sem Administrador do Sistema é recusada; solicitação por não-admin recusada.
- Contrato de `PUT /users/{id}/profile` inalterado (request/response por nome de perfil); front não muda.
- Perfil `SystemAdministrator` nasce fixo (IsFixed, Scope=System), resolvido por chave natural (IsFixed+Scope+Name), nunca pelo GUID.

## Evidências

- Backend: `rtk dotnet build` — 13 projetos, 0 erros. `rtk dotnet test` — **304/304 aprovados** (novos: `ProfileTests`/`PermissionTests` e `ModelBuildingTests`; `UserTests`, `SetUserProfileUseCaseTests`, `SetUserProfileValidatorTests`, `UserProfileClaimsTransformationTests` migrados para a entidade). `python scripts/check-harness.py` → `harness ok`.
- Modelo EF validado sem banco: `ModelBuildingTests` dispara `OnModelCreating` e confere que `Profile`/`Permission`/`ProfilePermission` mapeiam, que `User→Profile` é FK opcional por `ProfileId` e que `ProfilePermission` tem FK para `Profile` e `Permission` — cobre o alinhamento mapping↔migration que os testes mockados não alcançam.
- Convenções (ADR-052) exercidas no ajuste: enum de domínio com prefixo `E` (`EProfileScope`); entidade sem lambda capturante (gate EntityBase); enum como string por convenção global (ADR-031) → casa com `NVARCHAR` das migrations.
- Migrations: 4 arquivos (`Permissions`, `Profiles`, `ProfilePermissions`, `migrar-user-profile-para-entidade`) no padrão do repo (guards de existência, FK Restrict, seed do Perfil `SystemAdministrator` como dado de catálogo à la `LegalNatures`, backfill + drop da coluna `Users.Profile`).
- **NÃO aplicadas a nenhum banco nesta execução** (Docker daemon local indisponível). Diferente das entregas anteriores (ex.: 0006 aplicou a migration na VPS via sqlcmd, só o baseline Flyway ficou pendente): esta fatia está **abaixo do bar de verificação do harness** — `validar migration no banco de dev` (Passo 5) segue em aberto e é a migration mais arriscada da fatia (DDL + DML: seed, ALTER+FK, UPDATE, DROP). **Aplicar no banco de dev GATEIA o PR.**
- Contrato: `PUT /users/{id}/profile` inalterado (request/response por nome de perfil) — sem regeneração de `openapi.json` nem impacto no front.
- Débito técnico rastreado: `IX_Profiles_Name` é único global; RN-039/RN-040 exigem nome único **por Escopo** — a fatia 4 precisa de migration nova que substitua esse índice por composto/filtrado (registrado no `tech-debt-tracker.md`).
- Pendências: aplicar migrations no banco de dev (Docker/VPS) — gate do PR; AB#/PBI; PR (dbmigration → develop antes do backend → main); fatias 1..5 da jornada.
