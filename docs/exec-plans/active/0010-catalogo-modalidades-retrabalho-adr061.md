# Exec-plan 0010 — Catálogo de Modalidades: retrabalho para o modelo do ADR-061

Status: ativo — AB#0002 (`ab-0002-job-importar-modalidades`). Migra a implementação das fatias 1–3 (ADR-060) para o modelo do **ADR-061**.
Contexto obrigatório: `docs/adr/061-modalidade-derivada-da-global-modality.md`, RNs revistas em `docs/product-specs/regras-de-negocio/modalidades.md` (RN-029..036, 2026-07-22), glossário (revisto 2026-07-22), OPEN-09/10/11. Base a retrabalhar: exec-plans 0007/0008/0009 (histórico do modelo antigo).

## Objetivo

Passar de "dois mundos + Mapeamento próprio + Grupo no Smart + automação por identificador/semelhança" para: **Modalidade = Modalidade Global da OnPoint** (find-or-create por id global) **+ curadoria manual**; **vínculo `ImportedModality→Modality` direto** (id global) com **override manual preservado**; **sem `ModalityGroup`** e **sem `ModalityMapping`**; **Fila só para exceções** (importada sem id global) + ignorar/reativar/reatribuir.

## Backend (sub-agente A) — worktree `smartinsure-backend` + migration em `smartinsure-dbmigration`

- [ ] **Remover** `ModalityGroup` (entidade, enum `EModalityGroupStatus`, mapping, repositório, use cases ModalityGroupUseCases, `ModalityGroupsEndpoint`, DTOs) e seus testes.
- [ ] **Remover** `ModalityMapping` (entidade, enums `EMappingEstablishment`/`EModalityMappingStatus`, mapping, repositório, DTOs de mapa que dependiam dele) e os use cases de mapear que criavam mapping; e as RNs de fatia 2/3 relacionadas viram vínculo direto.
- [ ] **`Modality`**: remover `ModalityGroupId`; adicionar `GlobalModalityExternalId` (string, único quando presente); manter Name/Description/Status; factories `CreateManual(...)` e `CreateFromGlobal(externalId, name)`; `Rename/Update` só nome/descrição; Activate/Deactivate.
- [ ] **`ImportedModality`**: adicionar `ModalityId` (Guid?, FK) e `ModalityLinkSource` (`EModalityLinkSource { Automatic, Manual }`); método `LinkToModality(modalityId, source)` que **não sobrescreve** um vínculo `Manual` quando a importação (Automatic) roda; manter `EngineModalityId`/`EngineModalityName` (id/nome global), Branch, IsIgnored, params.
- [ ] **Importer**: para cada importada → find-or-create `Modality` por `EngineModalityId` (id global, nome global) e `LinkToModality(..., Automatic)` preservando override Manual; remover a lógica de mapping-por-identificador/semelhança; manter upsert (RN-030), desativação e resiliência (RN-035).
- [ ] **Migrations** (`smartinsure-dbmigration`, forward-only, guards): nova migration que (a) adiciona `Modalities.GlobalModalityExternalId` (único filtrado NOT NULL) e remove `Modalities.ModalityGroupId` (drop FK+coluna); (b) adiciona `ImportedModalities.ModalityId` (FK→Modalities) + `ModalityLinkSource`; (c) faz `DROP TABLE` de `dbo.ModalityMappings` e `dbo.ModalityGroups` (guarded). Sem reescrever migrations antigas.
- [ ] **Api**: remover `ModalityGroupsEndpoint`; `ModalitiesEndpoint` sem grupo; `ImportedModalitiesEndpoint` → `reassign` (set ModalityId Manual) + `ignore`/`restore`; `ModalityMapEndpoint` (GET) com o novo read-model (Modality → Seguradoras via `ImportedModality.ModalityId`; pendências = importadas sem ModalityId; disponibilidade por ramo). Manter `modality-imports/run`.
- [ ] **Read-model do Mapa**: matriz por Modalidade agregando Seguradoras distintas (uma por seguradora, com contagem — como corrigido no front); pendências = importadas ativas, não ignoradas, sem `ModalityId`.
- [ ] Testes `[Trait("RuleId","RN-029..036")]` reescritos para o modelo novo; `dotnet build`+`dotnet test` verdes; NetArchTest/Convention; `check-harness.py`; cobertura.
- [ ] Contrato `docs/generated/openapi.json` regenerado.
- [ ] Migração de dados legados: os mapeamentos/estruturas criados sob o ADR-060 no banco dev são substituídos pela nova migration + reimportação (`modality-imports/run`).

## Frontend (sub-agente B, após contrato regenerado)

- [ ] Remover a tela **Cadastro de Grupo de Modalidade** e a entrada de menu.
- [ ] **Cadastro de Modalidade**: sem campo Grupo.
- [ ] **Mapa de Modalidades**: consumir o novo contrato — matriz (uma badge por Seguradora, já corrigido) + Fila de **exceções** (importadas sem Modalidade) com ações **reatribuir/ignorar/reativar**; remover "mapear/promover" do modelo antigo onde não se aplica.
- [ ] `pnpm types:gen` (contrato novo); `pnpm lint`/`typecheck`/`test` + E2E das jornadas.

## Critérios de aceite

- Importação cria/atualiza Modalidades pela Modalidade Global e vincula as importadas automaticamente; override manual preservado na reimportação (RN-032).
- Não há `ModalityGroup` nem `ModalityMapping` no schema nem no código; Fila só mostra exceções + ignorar/reativar/reatribuir (RN-034).
- Modalidade oferecida com ≥1 importada ativa não ignorada vinculada; disponibilidade por ramo derivada (RN-033); preservação (RN-036); resiliência (RN-035).
- Gates verdes nos dois repos; contrato publicado; migration aplicada; teste ao vivo (reimport Bravo) mostrando "Judicial" como uma Modalidade única.

## Evidências

- (a preencher) build/test, migration, contrato, front, reimport ao vivo, commits.
