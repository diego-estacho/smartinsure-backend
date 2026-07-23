# Exec-plan 0009 — Vínculos do Usuário com Corretora/Tomador (RN-034, fatia 1)

Status: em andamento — fatia 1a da jornada Perfis e Permissões (slug provisório `perfis-permissoes`, AB# pendente). Sobre a fatia 0 (exec-plan 0008).
Contexto obrigatório: `AGENTS.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, RN-034 (`regras-de-negocio/perfis-e-permissoes.md`), glossário (termos `UserBrokerageMembership`/`UserPolicyHolderMembership` — ratificados 2026-07-23), ADR-060 (Escopo ativo por claim), ADR-035 (multi-tenant), [OPEN-11](../../product-specs/open-decisions.md).

## Objetivo

Modelar o vínculo N:N do Usuário com Corretoras e Tomadores (RN-034): um Usuário pode estar ligado a várias Corretoras e vários Tomadores, com um Perfil por vínculo. Esta fatia (1a) entrega apenas o **modelo** — entidades `UserBrokerageMembership`/`UserPolicyHolderMembership`, tabelas e repositórios. A resolução/troca do Escopo ativo e o gate de permissões efetivas (fatia 1b) dependem do ADR-060 e do padrão de primeiro acesso ainda aberto (OPEN-11); a criação de vínculos depende dos fluxos de hierarquia (fatia 3). Corretora e Tomador são `Person` (papéis Broker/PolicyHolder), referenciados por FK a `Persons`, como em `PolicyHolderAppointment`.

## Escopo e não-escopo (fatia 1a)

- **No escopo:** entidades e tabelas de vínculo (UserId + BrokerageId/PolicyHolderId + ProfileId), com FK a `Users`, `Persons` e `Profiles`; par único por Usuário×Corretora e Usuário×Tomador; repositórios + DI; testes de invariante.
- **Fora do escopo:** carregamento/troca do Escopo ativo por claim (fatia 1b — ADR-060, pendente do padrão de primeiro acesso, OPEN-11); endpoints de "listar minhas corretoras/selecionar ativa" (fatia 1b); criação de vínculos (fatia 3 — hierarquia); query filters multi-tenant por Corretora ativa (ADR-035, fatia 1b).

## Tarefas

- [ ] Migrations: `UserBrokerageMemberships` e `UserPolicyHolderMemberships` (FK `Users`/`Persons`/`Profiles`; par único).
- [ ] Core: entidades `UserBrokerageMembership`/`UserPolicyHolderMembership` (ricas: `Create`).
- [ ] Abstractions: `IUserBrokerageMembershipRepository`/`IUserPolicyHolderMembershipRepository` (existência do par, lista por Usuário).
- [ ] Infra.Data: mappings 1:1, repositórios, DbSets, DI.
- [ ] Testes de entidade + extensão do `ModelBuildingTests` (FKs dos vínculos).
- [ ] `dotnet build` + `dotnet test` + `check-harness.py` verdes.

## Critérios de aceite

- Vínculo carrega UserId, BrokerageId/PolicyHolderId (Person) e ProfileId; par único por Usuário×Corretora e Usuário×Tomador (constraint).
- Modelo EF constrói (OnModelCreating) com as FKs corretas; toda FK Restrict.
- Sem regra de Escopo ativo nem criação de vínculo nesta fatia (declarado como fora de escopo).

## Evidências

- Backend: `rtk dotnet build` — 13 projetos, 0 erros. `rtk dotnet test` — **308/308 aprovados** (novos: `MembershipTests` — RN-034; `ModelBuildingTests` estendido com as FKs dos dois vínculos). `python scripts/check-harness.py` → `harness ok`.
- Modelo EF validado sem banco: `UserBrokerageMembership`/`UserPolicyHolderMembership` mapeiam com FK para `User`, `Person` (Corretora/Tomador) e `Profile`.
- Migrations: `UserBrokerageMemberships` e `UserPolicyHolderMemberships` (FK `Users`/`Persons`/`Profiles`, par único por Usuário×Corretora e Usuário×Tomador) no padrão do repo. **NÃO aplicadas a banco** (empilham sobre a fatia 0 também não aplicada — ver 0008; aplicação gateia o PR).
- Escopo ativo (fatia 1b): mecânica decidida (claim, ADR-060); implementação pendente do Escopo padrão de primeiro acesso (OPEN-11, PO).
- Pendências: aplicar migrations no banco de dev (gate do PR) — **na ordem: aplicar as 4 da fatia 0 (0008) e CONFIRMAR antes de aplicar as 2 da 0009**, pois a migração de dados do 0008 (`DECLARE`/`EXEC`/ALTER+FK/UPDATE/DROP) nunca rodou e um erro dela não deve ficar soterrado sob as FKs da 0009; fatia 1b (após ratificar ADR-060 + padrão de primeiro acesso); AB#/PBI; PR.
