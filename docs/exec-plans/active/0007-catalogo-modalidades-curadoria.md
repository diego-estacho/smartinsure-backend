# Exec-plan 0007 — Catálogo de Modalidades, Fatia 1: Curadoria (RN-029, RN-036)

Status: ativo — AB#0002 (`ab-0002-job-importar-modalidades`). Em execução; PRs pendentes de validação.
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, `docs/adr/060-catalogo-modalidades-dois-mundos.md`, RNs em `docs/product-specs/regras-de-negocio/modalidades.md` (RN-029, RN-036), glossário (termos Modalidade, Grupo de Modalidade e status/enums — propostos 2026-07-21), OPEN-08/09/10 (fora desta fatia). Módulo análogo: catálogo de Seguradoras (exec-plan `completed/0003-catalogo-seguradoras.md`, `Insurer`/`InsurersEndpoint`).

## Objetivo

Entregar a curadoria do catálogo de Modalidades end-to-end: as entidades curadas **Grupo de Modalidade** (`ModalityGroup`) e **Modalidade** (`Modality`), com CRUD sem exclusão (criar/editar/inativar/reativar, RN-036), escrita restrita ao Administrador do Sistema (RN-029), e as duas telas de cadastro no front. NÃO entra nesta fatia: Modalidade Importada, mapeamento, importação (fatia 2), Mapa/Fila e disponibilidade derivada (fatia 3). A Modalidade nesta fatia não tem Ramo (trava é do lado importado, ADR-060).

## Tarefas

- [ ] Migration no `smartinsure-dbmigration`: `V{timestamp>20260720125347}__criar-tabelas-modality-catalog.sql` — `dbo.ModalityGroups` (Name único, Description, Status, DisplayOrder) e `dbo.Modalities` (Name único, ModalityGroupId FK → ModalityGroups, Description, Status); guards `IF OBJECT_ID`, PK GUID, Status `NVARCHAR(20)`, 4 colunas de auditoria, header citando RN-029/RN-036 + ADR-058.
- [ ] Core: entidades ricas `ModalityGroup` e `Modality` (`EntityBase`, ctor privado, factory `Create`, `Rename`/`Update`, `Activate`/`Deactivate` com conflito ao repetir estado — RN-036); enums `EModalityGroupStatus` e `EModalityStatus` (`Active`/`Inactive`) em `Core/Enumerators`.
- [ ] Core: `IModalityGroupRepository`, `IModalityRepository` (`NameExistsAsync`, `GetTrackedByIdAsync`, existência/atividade de grupo) + read DTOs em `Abstractions/Repositories/Dtos/`.
- [ ] Application: use cases `Create/Update/ChangeStatus/Get/List` para `ModalityGroup` e `Modality` (auto-registrados por convenção); validators FluentValidation (mensagens pt-BR): nome obrigatório/único, grupo obrigatório e existente, DisplayOrder ≥ 0.
- [ ] Infra.Data: `ModalityGroupMapping` e `ModalityMapping` (Fluent, 1:1 com a migration), `ModalityGroupRepository`, `ModalityRepository`; registrar `DbSet` + repositórios no `DependencyInjection.cs` (passo manual).
- [ ] Api: `ModalityGroupsEndpoint` (`modality-groups`) e `ModalitiesEndpoint` (`modalities`) — escrita com `.RequireAuthorization(Policies.SystemAdministrator)` (RN-029), leitura autenticada; `.Produces<T>()` para o contrato.
- [ ] Testes com `[Trait("RuleId","RN-029")]` e `[Trait("RuleId","RN-036")]` (xUnit/NSubstitute/FluentAssertions): criação, nome duplicado recusado, grupo inexistente recusado, inativar/reativar idempotente-negado, escrita negada sem Perfil.
- [ ] `dotnet build SmartInsure.slnx` + `dotnet test tests/SmartInsure.Tests` verdes (inclui NetArchTest/ConventionTests); cobertura ≥ 80%; `python scripts/check-harness.py` → `harness ok`.
- [ ] Validar migration localmente (`docker compose --profile migrations up -d`).
- [ ] Contrato `docs/generated/openapi.json` regenerado com os endpoints de Modalidade e Grupo.
- [ ] Frontend: telas Cadastro de Grupo de Modalidade e Cadastro de Modalidade (CRUD sem exclusão; status por nome estável; só design tokens); BFF + composables + `pnpm types:gen`; `pnpm lint && typecheck && test` verdes + E2E Playwright das jornadas.
- [ ] PRs vinculados por `AB#0002`: dbmigration (→ develop) antes do backend (→ main), depois frontend — não abrir até validação do Thiago.

## Critérios de aceite

- Administrador do Sistema cria/edita/inativa/reativa Grupo e Modalidade; usuário sem o Perfil recebe 403 na escrita (RN-029).
- Nome de Modalidade duplicado no catálogo é recusado (409); Modalidade sem Grupo ou com Grupo inexistente é recusada (RN-029).
- Inativar um item já Inativo (ou reativar já Ativo) é conflito de estado (RN-036); nenhum item é excluído fisicamente.
- Inativar um Grupo retira o Grupo e suas Modalidades da operação sem apagá-los (RN-036).
- Gates verdes: `dotnet build`/`dotnet test`, `check-harness.py`, cobertura ≥ 80%, migration aplicada localmente, contrato publicado.

## Evidências

- (a preencher ao concluir) Backend: saída de `dotnet build`/`dotnet test` (total e novos por RN); `check-harness.py`.
- (a preencher) Migration: Flyway local aplicou `criar-tabelas-modality-catalog`.
- (a preencher) Contrato: `docs/generated/openapi.json` com `/api/v1/modality-groups` e `/api/v1/modalities`.
- (a preencher) Front: `pnpm lint/typecheck/test` + E2E Playwright (gravação/screenshot no PR).
- Pendências: ratificação da PO (termos e RNs); PRs.
