# Exec-plan 0009 — Catálogo de Modalidades, Fatia 3: Mapa e Fila de Revisão (RN-036, RN-037)

Status: **superseded** (2026-07-23) — substituído pelo exec-plan `0010-catalogo-modalidades-retrabalho-adr061.md` (retrabalho para o modelo do ADR-061). Entregue sob o **ADR-060** e depois retrabalhado; mantido como histórico. **O modelo descrito abaixo (Fila de mapeamento, "mapeamento confirmado") NÃO é mais o vigente** — a Fila passou a tratar só exceções/curadoria; ver [ADR-061](../../adr/061-modalidade-derivada-da-global-modality.md) e RN-036/RN-037 (revistas 2026-07-22).
Contexto obrigatório: `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/adr/060-catalogo-modalidades-dois-mundos.md`, RNs em `docs/product-specs/regras-de-negocio/modalidades.md` (RN-036/RN-037), OPEN-08 (semelhança fora), fatias 1 (`Modality`/`ModalityGroup`) e 2 (`ImportedModality`/`ModalityMapping`, importação).

## Objetivo

Fechar o catálogo: o Mapa de Modalidades (matriz Seguradoras × Modalidades com pendências evidenciadas) e a Fila de Revisão como recorte de pendências, onde o Administrador do Sistema resolve cada Modalidade Importada não mapeada — mapear para Modalidade existente (Confirmado manual), promover (criar Modalidade e mapear) ou ignorar. Nada entra na operação sem mapeamento confirmado; disponibilidade e comparabilidade derivadas das Importadas ativas confirmadas (RN-036).

## Escopo e cortes

- Disponibilidade derivada **por ramo** (público/privado) a partir do `Branch` das Importadas ativas confirmadas. A derivação **PF/PJ** depende da semântica dos flags do PlugV2 (`IgnoreBranchWhenInsuredIsPF`/`IgnoreBranchWhenInsuredIsPrivate`), **não definida** → registrar OPEN e não implementar (não inventar regra).
- Semelhança automática segue fora (OPEN-08).

## Tarefas

- [ ] Migration `V20260722084643__adicionar-ignored-em-imported-modalities.sql`: `IsIgnored BIT NOT NULL DEFAULT 0` (guard `COL_LENGTH`).
- [ ] Core: `ImportedModality.Ignore()`/`Restore()` (marcador Ignorada, RN-037; a importação não mexe nele — não volta à fila); `ModalityMapping.CreateManual(importedModalityId, modalityId, confirmedBy, confirmedAt)` → Confirmado/Manual.
- [ ] Core: repositórios — pendências da Fila (Importadas Ativas, não Ignoradas, sem mapeamento Confirmado); ramo já confirmado de uma Modalidade (trava, RN-037); leitura do Mapa (matriz + disponibilidade por ramo).
- [ ] Application: `MapImportedModalityUseCase` (mapear pendência → Modalidade existente, trava de ramo), `IgnoreImportedModalityUseCase` (marcar Ignorada). Promover = `CreateModality` (fatia 1) + Map. `GetModalityMapUseCase` (matriz + pendências + disponibilidade por ramo). Validators.
- [ ] Infra.Data: mappings/DbSet (IsIgnored), repositórios, DI.
- [ ] Api: `GET /modality-map`; `POST /imported-modalities/{id}/map`; `POST /imported-modalities/{id}/ignore` — escrita restrita ao Administrador do Sistema (RN-032/011).
- [ ] Testes `[Trait("RuleId","RN-036/034")]`: mapear confirma manual; trava de ramo recusa; ignorar remove da fila e não é oferecida; leitura do Mapa (oferecida só com ≥1 confirmada ativa; disponibilidade por ramo).
- [ ] `dotnet build`/`dotnet test` verdes, `check-harness.py` ok, migration aplicada, contrato regenerado.
- [x] Frontend: tela **Mapa de Modalidades** (`app/pages/mapa-de-modalidades/`) — matriz Seguradoras × Modalidades + Fila de Revisão na mesma tela, ações mapear/promover/ignorar; BFF proxiando, types regenerados do contrato; `pnpm lint`/`typecheck` verdes, `pnpm test` 113/113, 2 E2E novos verdes.

## Critérios de aceite

- Mapear pendência confirma o mapeamento (Manual, com quem/quando); ramo incompatível é recusado (RN-037).
- Ignorar retira a Importada da Fila e da operação; não reaparece na fila nas próximas importações; pode ser reavaliada (RN-037).
- Promover cria a Modalidade e mapeia a pendência (nome único, RN-032).
- Modalidade só é "oferecida" com ≥1 Modalidade Importada Ativa com mapeamento Confirmado; disponibilidade por ramo derivada (RN-036).
- Gates verdes; contrato publicado; tela do Mapa verde.

## Evidências

- **Backend** (2026-07-22): `dotnet build` 0 erros; `dotnet test` **322/322** (fatia 3 +6: mapear manual/trava de ramo/já-mapeada, ignorar/não-encontrada, Mapa oferta+ramo); `check-harness.py` → `harness ok`.
- **Migration**: Flyway aplicou `V20260722084643 - adicionar-ignored-em-imported-modalities` no docker (schema v20260722084643).
- **Contrato**: `docs/generated/openapi.json` (32 rotas) com `GET /modality-map`, `POST /imported-modalities/{id}/map` e `/ignore`.
- **Front** (worktree `smartinsure-frontend`, branch `ab-0002-job-importar-modalidades`): tela Mapa de Modalidades + Fila espelhando o slice de fatia 1; BFF, composable, mapa de Ramo por nome estável, dialogs mapear/promover(reusa cadastro)/ignorar. Gates re-verificados no run principal: `pnpm lint` exit 0, `pnpm typecheck` exit 0, `pnpm test` **113/113 (22 arquivos)**, 2 E2E novos verdes. Falhas do E2E completo (login/smoke/tomadores) pré-existentes — branch não toca esses specs. 5 commits pt-BR `AB#0002`, sem PR.
- **Corte honesto**: disponibilidade **PF/PJ** não implementada (semântica dos flags PlugV2 indefinida) → **OPEN-11**; disponibilidade **por ramo** entregue (derivada do `Branch`).
- **Commits backend**: `feat: Mapa de Modalidades e Fila de Revisão` + contrato; dbmigration `V20260722084643`. Front: 5 commits (`13906f1`..`c9013e9`).
- Pendências: ratificação da PO; abertura dos PRs (dbmigration → develop, backend → main, frontend); OPEN-11 (PF/PJ); OPEN-08 (semelhança automática).
