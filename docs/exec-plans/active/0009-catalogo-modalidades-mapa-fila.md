# Exec-plan 0009 — Catálogo de Modalidades, Fatia 3: Mapa e Fila de Revisão (RN-033, RN-034)

Status: ativo — AB#0002 (`ab-0002-job-importar-modalidades`). Em execução.
Contexto obrigatório: `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/adr/060-catalogo-modalidades-dois-mundos.md`, RNs em `docs/product-specs/regras-de-negocio/modalidades.md` (RN-033/RN-034), OPEN-08 (semelhança fora), fatias 1 (`Modality`/`ModalityGroup`) e 2 (`ImportedModality`/`ModalityMapping`, importação).

## Objetivo

Fechar o catálogo: o Mapa de Modalidades (matriz Seguradoras × Modalidades com pendências evidenciadas) e a Fila de Revisão como recorte de pendências, onde o Administrador do Sistema resolve cada Modalidade Importada não mapeada — mapear para Modalidade existente (Confirmado manual), promover (criar Modalidade e mapear) ou ignorar. Nada entra na operação sem mapeamento confirmado; disponibilidade e comparabilidade derivadas das Importadas ativas confirmadas (RN-033).

## Escopo e cortes

- Disponibilidade derivada **por ramo** (público/privado) a partir do `Branch` das Importadas ativas confirmadas. A derivação **PF/PJ** depende da semântica dos flags do PlugV2 (`IgnoreBranchWhenInsuredIsPF`/`IgnoreBranchWhenInsuredIsPrivate`), **não definida** → registrar OPEN e não implementar (não inventar regra).
- Semelhança automática segue fora (OPEN-08).

## Tarefas

- [ ] Migration `V20260722084643__adicionar-ignored-em-imported-modalities.sql`: `IsIgnored BIT NOT NULL DEFAULT 0` (guard `COL_LENGTH`).
- [ ] Core: `ImportedModality.Ignore()`/`Restore()` (marcador Ignorada, RN-034; a importação não mexe nele — não volta à fila); `ModalityMapping.CreateManual(importedModalityId, modalityId, confirmedBy, confirmedAt)` → Confirmado/Manual.
- [ ] Core: repositórios — pendências da Fila (Importadas Ativas, não Ignoradas, sem mapeamento Confirmado); ramo já confirmado de uma Modalidade (trava, RN-034); leitura do Mapa (matriz + disponibilidade por ramo).
- [ ] Application: `MapImportedModalityUseCase` (mapear pendência → Modalidade existente, trava de ramo), `IgnoreImportedModalityUseCase` (marcar Ignorada). Promover = `CreateModality` (fatia 1) + Map. `GetModalityMapUseCase` (matriz + pendências + disponibilidade por ramo). Validators.
- [ ] Infra.Data: mappings/DbSet (IsIgnored), repositórios, DI.
- [ ] Api: `GET /modality-map`; `POST /imported-modalities/{id}/map`; `POST /imported-modalities/{id}/ignore` — escrita restrita ao Administrador do Sistema (RN-029/011).
- [ ] Testes `[Trait("RuleId","RN-033/034")]`: mapear confirma manual; trava de ramo recusa; ignorar remove da fila e não é oferecida; leitura do Mapa (oferecida só com ≥1 confirmada ativa; disponibilidade por ramo).
- [ ] `dotnet build`/`dotnet test` verdes, `check-harness.py` ok, migration aplicada, contrato regenerado.
- [ ] Frontend (subagente): tela **Mapa de Modalidades** — matriz Seguradoras × Modalidades, pendências evidenciadas na própria exibição (Fila), ações mapear/promover/ignorar; consome types do contrato; `pnpm lint/typecheck/test` + E2E.

## Critérios de aceite

- Mapear pendência confirma o mapeamento (Manual, com quem/quando); ramo incompatível é recusado (RN-034).
- Ignorar retira a Importada da Fila e da operação; não reaparece na fila nas próximas importações; pode ser reavaliada (RN-034).
- Promover cria a Modalidade e mapeia a pendência (nome único, RN-029).
- Modalidade só é "oferecida" com ≥1 Modalidade Importada Ativa com mapeamento Confirmado; disponibilidade por ramo derivada (RN-033).
- Gates verdes; contrato publicado; tela do Mapa verde.

## Evidências

- (a preencher) build/test, migration, contrato, front (gates + E2E), commits.
