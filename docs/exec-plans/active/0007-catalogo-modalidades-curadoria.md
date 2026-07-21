# Exec-plan 0007 — Catálogo de Modalidades, Fatia 1: Curadoria (RN-029, RN-036)

Status: ativo — AB#0002 (`ab-0002-job-importar-modalidades`). Em execução; PRs pendentes de validação.
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, `docs/adr/060-catalogo-modalidades-dois-mundos.md`, RNs em `docs/product-specs/regras-de-negocio/modalidades.md` (RN-029, RN-036), glossário (termos Modalidade, Grupo de Modalidade e status/enums — propostos 2026-07-21), OPEN-08/09/10 (fora desta fatia). Módulo análogo: catálogo de Seguradoras (exec-plan `completed/0003-catalogo-seguradoras.md`, `Insurer`/`InsurersEndpoint`).

## Objetivo

Entregar a curadoria do catálogo de Modalidades end-to-end: as entidades curadas **Grupo de Modalidade** (`ModalityGroup`) e **Modalidade** (`Modality`), com CRUD sem exclusão (criar/editar/inativar/reativar, RN-036), escrita restrita ao Administrador do Sistema (RN-029), e as duas telas de cadastro no front. NÃO entra nesta fatia: Modalidade Importada, mapeamento, importação (fatia 2), Mapa/Fila e disponibilidade derivada (fatia 3). A Modalidade nesta fatia não tem Ramo (trava é do lado importado, ADR-060).

## Tarefas

- [x] Migration no `smartinsure-dbmigration`: `V20260721200608__criar-tabelas-modality-catalog.sql` — `dbo.ModalityGroups` (Name único, Description, Status, DisplayOrder) e `dbo.Modalities` (Name único, ModalityGroupId FK → ModalityGroups, Description, Status); guards `IF OBJECT_ID`, PK GUID, Status `NVARCHAR(20)`, 4 colunas de auditoria, header citando RN-029/RN-036 + ADR-058.
- [x] Core: entidades ricas `ModalityGroup` e `Modality` (`EntityBase`, ctor privado, factory `Create`, `Update`, `Activate`/`Deactivate` com conflito ao repetir estado — RN-036); enums `EModalityGroupStatus` e `EModalityStatus` (`Active`/`Inactive`) em `Core/Enumerators`.
- [x] Core: `IModalityGroupRepository`, `IModalityRepository` (`NameExistsAsync`, `ListAsync`) + read DTOs em `Abstractions/Repositories/Dtos/`.
- [x] Application: use cases `Create/Update/ChangeStatus/Get/List` para `ModalityGroup` e `Modality` (auto-registrados por convenção); validators FluentValidation (mensagens pt-BR): nome obrigatório/único, grupo obrigatório e existente, DisplayOrder ≥ 0.
- [x] Infra.Data: `ModalityGroupMapping` e `ModalityMapping` (Fluent, 1:1 com a migration), `ModalityGroupRepository`, `ModalityRepository`; `DbSet` + repositórios registrados no `DependencyInjection.cs`.
- [x] Api: `ModalityGroupsEndpoint` (`modality-groups`) e `ModalitiesEndpoint` (`modalities`) — escrita com `.RequireAuthorization(Policies.SystemAdministrator)` (RN-029), leitura autenticada; `.Produces<T>()` para o contrato.
- [x] Testes com `[Trait("RuleId","RN-029")]` e `[Trait("RuleId","RN-036")]` (xUnit/NSubstitute/FluentAssertions): criação, nome duplicado recusado, grupo inexistente recusado, inativar/reativar idempotente-negado, validators de forma.
- [x] `dotnet build SmartInsure.slnx` + `dotnet test tests/SmartInsure.Tests` verdes (inclui NetArchTest/ConventionTests); `python scripts/check-harness.py` → `harness ok`. Cobertura: ver Evidências (nova lógica bem coberta; gate agregado é pré-existente).
- [x] Validar migration localmente (`docker compose --profile migrations up -d` + `repair`/`migrate`).
- [ ] **PENDENTE** — Contrato `docs/generated/openapi.json` regenerado com os endpoints de Modalidade e Grupo (endpoints já anotados com `.Produces<>`; regen exige subir a API — fazer em dev/CI, nunca editar à mão o arquivo derivado).
- [ ] **PENDENTE** — Frontend: telas Cadastro de Grupo de Modalidade e Cadastro de Modalidade (CRUD sem exclusão; status por nome estável; só design tokens); BFF + composables + `pnpm types:gen`; `pnpm lint && typecheck && test` verdes + E2E Playwright das jornadas.
- [ ] **PENDENTE** — PRs vinculados por `AB#0002`: dbmigration (→ develop) antes do backend (→ main), depois frontend — não abrir até validação do Thiago.

## Critérios de aceite

- Administrador do Sistema cria/edita/inativa/reativa Grupo e Modalidade; usuário sem o Perfil recebe 403 na escrita (RN-029).
- Nome de Modalidade duplicado no catálogo é recusado (409); Modalidade sem Grupo ou com Grupo inexistente é recusada (RN-029).
- Inativar um item já Inativo (ou reativar já Ativo) é conflito de estado (RN-036); nenhum item é excluído fisicamente.
- Inativar um Grupo retira o Grupo e suas Modalidades da operação sem apagá-los (RN-036).
- Gates verdes: `dotnet build`/`dotnet test`, `check-harness.py`, cobertura ≥ 80%, migration aplicada localmente, contrato publicado.

## Evidências

- **Backend** (2026-07-21): `dotnet build SmartInsure.slnx` → 0 erros (warnings pré-existentes: `CarterModule` obsoleto em todos os endpoints, CS8602 em testes legados). `dotnet test tests/SmartInsure.Tests` → **301/301 aprovados** (partindo de 273 do baseline; +28 novos: entidades RN-029/RN-036, use cases Create/Update/ChangeStatus/Get/List dos dois agregados, validators). `python scripts/check-harness.py` → `harness ok`.
- **Cobertura** (coverlet): `SmartInsure.Core` 86%; classes novas de use case e entidade em 0,8–1,0 (medido por classe no cobertura.xml). Agregado do assembly `SmartInsure.Application.UseCase` em 69% — reflete código **pré-existente** sem teste no assembly (serviços/use cases anteriores), não as adições desta fatia. O gate de 80% do CI ainda é esboço no `azure-pipelines.yml` (não implementado); recomendação registrada para quando o step existir.
- **Migration**: Flyway local aplicou `20260721200608 - criar-tabelas-modality-catalog` com sucesso (`repair` + `migrate`; schema agora em v20260721200608). O mismatch de checksum encontrado era de migrations pré-existentes (finais de linha CRLF/LF no volume local), não da nova.
- **Commits** (branch `ab-0002-job-importar-modalidades`, sem PR): backend `dedcee3` (docs/RN/ADR), `a1d1e95` (entidades), `3b2ca48` (persistência+use cases), `59b912d` (endpoints), `+ test` (validators); dbmigration `e5b6ef5` (migration).
- **PENDENTE — Contrato**: `docs/generated/openapi.json` a regenerar (endpoints anotados com `.Produces<>`; arquivo é derivado — regen ao subir a API em dev/CI, nunca à mão).
- **PENDENTE — Front**: telas de cadastro + `pnpm types:gen` + `pnpm lint/typecheck/test` + E2E (gravação/screenshot no PR).
- Pendências de processo: ratificação da PO (termos e RNs propostos); abertura dos PRs após validação do Thiago.
