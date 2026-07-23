# Exec-plan 0007 — Catálogo de Modalidades, Fatia 1: Curadoria (RN-032, RN-039)

Status: **superseded** (2026-07-23) — substituído pelo exec-plan `0010-catalogo-modalidades-retrabalho-adr061.md` (retrabalho para o modelo do ADR-061). Entregue sob o **ADR-060** e depois retrabalhado; mantido como histórico. **O modelo descrito abaixo (Grupo de Modalidade, Mapeamento) NÃO é mais o vigente** — ver [ADR-061](../../adr/061-modalidade-derivada-da-global-modality.md) e RN-032..039 (revistas 2026-07-22).
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, `docs/adr/060-catalogo-modalidades-dois-mundos.md`, RNs em `docs/product-specs/regras-de-negocio/modalidades.md` (RN-032, RN-039), glossário (termos Modalidade, Grupo de Modalidade e status/enums — propostos 2026-07-21), OPEN-08/09/10 (fora desta fatia). Módulo análogo: catálogo de Seguradoras (exec-plan `completed/0003-catalogo-seguradoras.md`, `Insurer`/`InsurersEndpoint`).

## Objetivo

Entregar a curadoria do catálogo de Modalidades end-to-end: as entidades curadas **Grupo de Modalidade** (`ModalityGroup`) e **Modalidade** (`Modality`), com CRUD sem exclusão (criar/editar/inativar/reativar, RN-039), escrita restrita ao Administrador do Sistema (RN-032), e as duas telas de cadastro no front. NÃO entra nesta fatia: Modalidade Importada, mapeamento, importação (fatia 2), Mapa/Fila e disponibilidade derivada (fatia 3). A Modalidade nesta fatia não tem Ramo (trava é do lado importado, ADR-060).

## Tarefas

- [x] Migration no `smartinsure-dbmigration`: `V20260721200608__criar-tabelas-modality-catalog.sql` — `dbo.ModalityGroups` (Name único, Description, Status, DisplayOrder) e `dbo.Modalities` (Name único, ModalityGroupId FK → ModalityGroups, Description, Status); guards `IF OBJECT_ID`, PK GUID, Status `NVARCHAR(20)`, 4 colunas de auditoria, header citando RN-032/RN-039 + ADR-058.
- [x] Core: entidades ricas `ModalityGroup` e `Modality` (`EntityBase`, ctor privado, factory `Create`, `Update`, `Activate`/`Deactivate` com conflito ao repetir estado — RN-039); enums `EModalityGroupStatus` e `EModalityStatus` (`Active`/`Inactive`) em `Core/Enumerators`.
- [x] Core: `IModalityGroupRepository`, `IModalityRepository` (`NameExistsAsync`, `ListAsync`) + read DTOs em `Abstractions/Repositories/Dtos/`.
- [x] Application: use cases `Create/Update/ChangeStatus/Get/List` para `ModalityGroup` e `Modality` (auto-registrados por convenção); validators FluentValidation (mensagens pt-BR): nome obrigatório/único, grupo obrigatório e existente, DisplayOrder ≥ 0.
- [x] Infra.Data: `ModalityGroupMapping` e `ModalityMapping` (Fluent, 1:1 com a migration), `ModalityGroupRepository`, `ModalityRepository`; `DbSet` + repositórios registrados no `DependencyInjection.cs`.
- [x] Api: `ModalityGroupsEndpoint` (`modality-groups`) e `ModalitiesEndpoint` (`modalities`) — escrita com `.RequireAuthorization(Policies.SystemAdministrator)` (RN-032), leitura autenticada; `.Produces<T>()` para o contrato.
- [x] Testes com `[Trait("RuleId","RN-032")]` e `[Trait("RuleId","RN-039")]` (xUnit/NSubstitute/FluentAssertions): criação, nome duplicado recusado, grupo inexistente recusado, inativar/reativar idempotente-negado, validators de forma.
- [x] `dotnet build SmartInsure.slnx` + `dotnet test tests/SmartInsure.Tests` verdes (inclui NetArchTest/ConventionTests); `python scripts/check-harness.py` → `harness ok`. Cobertura: ver Evidências (nova lógica bem coberta; gate agregado é pré-existente).
- [x] Validar migration localmente (`docker compose --profile migrations up -d` + `repair`/`migrate`).
- [x] Contrato `docs/generated/openapi.json` regenerado (API subida com config mockada + SQL do docker; `/openapi/v1.json` capturado) — inclui as 6 rotas de Modalidade/Grupo e os schemas Create/Update/ChangeStatus/Get/List.
- [x] Frontend: telas Cadastro de Grupo de Modalidade e Cadastro de Modalidade (CRUD sem exclusão; status por nome estável; só design tokens); BFF Nitro + composables + types regenerados do contrato; `pnpm lint`/`typecheck` verdes, `pnpm test` 104/104, 2 E2E Playwright novos verdes.
- [ ] **PENDENTE (aguarda Thiago)** — PRs vinculados por `AB#0002`: dbmigration (→ develop) antes do backend (→ main), depois frontend — não abrir até validação final.

## Critérios de aceite

- Administrador do Sistema cria/edita/inativa/reativa Grupo e Modalidade; usuário sem o Perfil recebe 403 na escrita (RN-032).
- Nome de Modalidade duplicado no catálogo é recusado (409); Modalidade sem Grupo ou com Grupo inexistente é recusada (RN-032).
- Inativar um item já Inativo (ou reativar já Ativo) é conflito de estado (RN-039); nenhum item é excluído fisicamente.
- Inativar um Grupo retira o Grupo e suas Modalidades da operação sem apagá-los (RN-039).
- Gates verdes: `dotnet build`/`dotnet test`, `check-harness.py`, cobertura ≥ 80%, migration aplicada localmente, contrato publicado.

## Evidências

- **Backend** (2026-07-21): `dotnet build SmartInsure.slnx` → 0 erros (warnings pré-existentes: `CarterModule` obsoleto em todos os endpoints, CS8602 em testes legados). `dotnet test tests/SmartInsure.Tests` → **301/301 aprovados** (partindo de 273 do baseline; +28 novos: entidades RN-032/RN-039, use cases Create/Update/ChangeStatus/Get/List dos dois agregados, validators). `python scripts/check-harness.py` → `harness ok`.
- **Cobertura** (coverlet): `SmartInsure.Core` 86%; classes novas de use case e entidade em 0,8–1,0 (medido por classe no cobertura.xml). Agregado do assembly `SmartInsure.Application.UseCase` em 69% — reflete código **pré-existente** sem teste no assembly (serviços/use cases anteriores), não as adições desta fatia. O gate de 80% do CI ainda é esboço no `azure-pipelines.yml` (não implementado); recomendação registrada para quando o step existir.
- **Migration**: Flyway local aplicou `20260721200608 - criar-tabelas-modality-catalog` com sucesso (`repair` + `migrate`; schema agora em v20260721200608). O mismatch de checksum encontrado era de migrations pré-existentes (finais de linha CRLF/LF no volume local), não da nova.
- **Commits** (branch `ab-0002-job-importar-modalidades`, sem PR): backend `dedcee3` (docs/RN/ADR), `a1d1e95` (entidades), `3b2ca48` (persistência+use cases), `59b912d` (endpoints), `+ test` (validators); dbmigration `e5b6ef5` (migration).
- **Contrato**: `docs/generated/openapi.json` regenerado subindo a `SmartInsure.Api` (Development, config mockada nas Options `ValidateOnStart`, SQL do docker) e capturando `/openapi/v1.json`. Publica `/api/v1/modality-groups` e `/api/v1/modalities` (GET list/detail, POST, PUT, PATCH /status) + schemas. JSON válido.
- **Front** (2026-07-21, worktree `smartinsure-frontend`, branch `ab-0002-job-importar-modalidades`): telas Cadastro de Grupo de Modalidade e Cadastro de Modalidade espelhando o slice de Corretoras/Habilitações; BFF Nitro proxiando as 6 rotas, composables, mapas de status por nome estável, dropdown de Grupo no form de Modalidade; types regenerados do `openapi.json` (nunca à mão). Gates re-verificados no run principal: `pnpm lint` exit 0, `pnpm typecheck` exit 0, `pnpm test` **104/104 (19 arquivos)**, 2 E2E novos verdes (criar + inativar por tela). Falhas no E2E completo (login/smoke/tomadores) são pré-existentes — confirmado que a branch não toca esses specs (`git diff main...HEAD` vazio neles). 6 commits pt-BR com `AB#0002`, sem PR.
- **Validação** (2026-07-21): Thiago validou RN/design e autorizou seguir com a implementação (inclui a escrita restrita ao Administrador do Sistema). O flip formal do status "proposto" no glossário/RN fica a cargo do dono quando for o caso.
- Pendência de processo: abertura dos PRs (dbmigration → develop, backend → main, frontend) — deixada para o Thiago acionar após a revisão final.
