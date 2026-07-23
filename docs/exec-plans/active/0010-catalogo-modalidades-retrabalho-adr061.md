# Exec-plan 0010 — Catálogo de Modalidades: retrabalho para o modelo do ADR-061

Status: ativo — AB#0002 (`ab-0002-job-importar-modalidades`). **Entregue e verificado em 2026-07-22** (backend + migration + front, gates verdes, reimport Bravo ao vivo); PRs de AB#0002 pendentes. É o registro vigente do domínio de Modalidades sob o **ADR-061**. Migrou a implementação das fatias 1–3 (ADR-060) para o modelo do ADR-061.
Contexto obrigatório: `docs/adr/061-modalidade-derivada-da-global-modality.md`, RNs revistas em `docs/product-specs/regras-de-negocio/modalidades.md` (RN-029..036, 2026-07-22), glossário (revisto 2026-07-22), OPEN-09/10/11. Base retrabalhada (histórico do modelo antigo, superseded): exec-plans `completed/0007`, `completed/0008`, `completed/0009`.

## Objetivo

Passar de "dois mundos + Mapeamento próprio + Grupo no Smart + automação por identificador/semelhança" para: **Modalidade = Modalidade Global da OnPoint** (find-or-create por id global) **+ curadoria manual**; **vínculo `ImportedModality→Modality` direto** (id global) com **override manual preservado**; **sem `ModalityGroup`** e **sem `ModalityMapping`**; **Fila só para exceções** (importada sem id global) + ignorar/reativar/reatribuir.

## Backend (sub-agente A) — worktree `smartinsure-backend` + migration em `smartinsure-dbmigration`

- [x] **Remover** `ModalityGroup` (entidade, enum `EModalityGroupStatus`, mapping, repositório, use cases ModalityGroupUseCases, `ModalityGroupsEndpoint`, DTOs) e seus testes.
- [x] **Remover** `ModalityMapping` (entidade, enums `EMappingEstablishment`/`EModalityMappingStatus`, mapping, repositório, DTOs de mapa que dependiam dele) e os use cases de mapear que criavam mapping; e as RNs de fatia 2/3 relacionadas viram vínculo direto.
- [x] **`Modality`**: remover `ModalityGroupId`; adicionar `GlobalModalityExternalId` (string, único quando presente); manter Name/Description/Status; factories `CreateManual(...)` e `CreateFromGlobal(externalId, name)`; `Rename/Update` só nome/descrição; Activate/Deactivate.
- [x] **`ImportedModality`**: adicionar `ModalityId` (Guid?, FK) e `ModalityLinkSource` (`EModalityLinkSource { Automatic, Manual }`); método `LinkToModality(modalityId, source)` que **não sobrescreve** um vínculo `Manual` quando a importação (Automatic) roda; manter `EngineModalityId`/`EngineModalityName` (id/nome global), Branch, IsIgnored, params.
- [x] **Importer**: para cada importada → find-or-create `Modality` por `EngineModalityId` (id global, nome global) e `LinkToModality(..., Automatic)` preservando override Manual; remover a lógica de mapping-por-identificador/semelhança; manter upsert (RN-030), desativação e resiliência (RN-035).
- [x] **Migrations** (`smartinsure-dbmigration`, forward-only, guards): nova migration que (a) adiciona `Modalities.GlobalModalityExternalId` (único filtrado NOT NULL) e remove `Modalities.ModalityGroupId` (drop FK+coluna); (b) adiciona `ImportedModalities.ModalityId` (FK→Modalities) + `ModalityLinkSource`; (c) faz `DROP TABLE` de `dbo.ModalityMappings` e `dbo.ModalityGroups` (guarded). Sem reescrever migrations antigas.
- [x] **Api**: remover `ModalityGroupsEndpoint`; `ModalitiesEndpoint` sem grupo; `ImportedModalitiesEndpoint` → `reassign` (set ModalityId Manual) + `ignore`/`restore`; `ModalityMapEndpoint` (GET) com o novo read-model (Modality → Seguradoras via `ImportedModality.ModalityId`; pendências = importadas sem ModalityId; disponibilidade por ramo). Manter `modality-imports/run`.
- [x] **Read-model do Mapa**: matriz por Modalidade agregando Seguradoras distintas (uma por seguradora, com contagem — como corrigido no front); pendências = importadas ativas, não ignoradas, sem `ModalityId`.
- [x] Testes `[Trait("RuleId","RN-029..036")]` reescritos para o modelo novo; `dotnet build`+`dotnet test` verdes; NetArchTest/Convention; `check-harness.py`; cobertura.
- [x] Contrato `docs/generated/openapi.json` regenerado.
- [x] Migração de dados legados: os mapeamentos/estruturas criados sob o ADR-060 no banco dev são substituídos pela nova migration + reimportação (`modality-imports/run`).

## Frontend (sub-agente B, após contrato regenerado)

- [x] Removida a tela **Cadastro de Grupo de Modalidade** e a entrada de menu (páginas, componentes, composable, BFF, status, testes).
- [x] **Cadastro de Modalidade**: sem campo Grupo (form, tabela, composable, payloads BFF).
- [x] **Mapa de Modalidades**: consome o contrato novo — matriz com uma badge por Seguradora (agregada pelo backend; removido o agrupamento client-side) + Fila de **exceções** com ações **reatribuir/ignorar/reativar**; removidos mapear/promover.
- [x] Types regenerados do contrato novo; `pnpm lint`/`typecheck` verdes, `pnpm test` 106/106, 2 E2E de modalidade verdes (6 testes).

## Critérios de aceite

- Importação cria/atualiza Modalidades pela Modalidade Global e vincula as importadas automaticamente; override manual preservado na reimportação (RN-032).
- Não há `ModalityGroup` nem `ModalityMapping` no schema nem no código; Fila só mostra exceções + ignorar/reativar/reatribuir (RN-034).
- Modalidade oferecida com ≥1 importada ativa não ignorada vinculada; disponibilidade por ramo derivada (RN-033); preservação (RN-036); resiliência (RN-035).
- Gates verdes nos dois repos; contrato publicado; migration aplicada; teste ao vivo (reimport Bravo) mostrando "Judicial" como uma Modalidade única.

## Evidências

### Backend (AB#0002) — concluído em 2026-07-22

- **Build**: `dotnet build SmartInsure.slnx` → 0 erros.
- **Testes**: `dotnet test tests/SmartInsure.Tests` → `Passed! - Failed: 0, Passed: 299, Skipped: 0, Total: 299` (inclui NetArchTest/ConventionTests). Testes de modalidades reescritos para o modelo novo; testes de `ModalityGroup`/`ModalityMapping` removidos.
- **Harness**: `python scripts/check-harness.py` → `harness ok`.
- **Migration**: `smartinsure-dbmigration/migrations/V20260722170658__modalidade-derivada-da-global-modality.sql` aplicada via `docker compose --profile migrations run --rm flyway repair` + `migrate` → `Successfully applied 1 migration ... now at version v20260722170658`. Schema conferido: `ModalityGroups`/`ModalityMappings` inexistentes; `Modalities.GlobalModalityExternalId` presente + `Modalities.ModalityGroupId` removido; `ImportedModalities.ModalityId`/`ModalityLinkSource` presentes.
- **Contrato**: `docs/generated/openapi.json` regenerado (servers normalizado p/ `http://127.0.0.1:5981/`). Rotas: `imported-modalities/{id}/reassign|ignore|restore`, `modalities` sem grupo, `modality-map`, `modality-imports/run`; sem `modality-groups`; `MapInsurerResponse` = insurerId/insurerName/count/origins.
- **Reimport ao vivo** (`POST /api/v1/modality-imports/run`, admin@dev.local): `insurersProcessed 2, succeeded 2, failed 0`. "Judicial" é **uma** Modalidade (id global 31) com Essor (10 importadas) e Excelsior (2 importadas) vinculadas `Automatic`; Mapa agrega uma badge por Seguradora com contagem; Fila vazia. Cruft legado (4 Modalidades manuais do ADR-060 sem id global, ex.: "Licitante") removido do banco dev por colidir com o nome único das derivadas — superseded pela reimportação.

### Frontend (sub-agente B, 2026-07-22)

- Worktree `smartinsure-frontend`, commit `65de640` (`AB#0002`, sem PR). Removidos: tela/menu de Grupo de Modalidade, `map.post` e o agrupamento client-side (`modalityMap.ts`). Adicionados: BFF `reassign`/`restore`, `ReassignDialog`. Ajustados: Mapa (badge por Seguradora do contrato agregado), Fila (reatribuir/ignorar/reativar), Cadastro de Modalidade sem Grupo, types regenerados.
- Gates (re-verificados no run principal): `pnpm lint` exit 0, `pnpm typecheck` exit 0, `pnpm test` **106/106 (20 arquivos)**, E2E de modalidade **6 passed**. Falhas pré-existentes (login/smoke/tomadores) não tocadas.
- **Edge registrado**: nome único de Modalidade × identidade por id global → OPEN-13.
