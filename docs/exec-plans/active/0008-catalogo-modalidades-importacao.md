# Exec-plan 0008 — Catálogo de Modalidades, Fatia 2: Importação (RN-030, RN-031, RN-032, RN-035)

Status: ativo — AB#0002 (`ab-0002-job-importar-modalidades`). Em execução.
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/adr/060-catalogo-modalidades-dois-mundos.md`, RNs em `docs/product-specs/regras-de-negocio/modalidades.md` (RN-030/031/032/035), spec de produto (Anexo A — contrato PlugV2), OPEN-08 (semelhança fora de escopo), OPEN-09 (credencial multi-corretora), OPEN-10 (cadência). Base: fatia 1 (exec-plan 0007, entidades `Modality`/`ModalityGroup`) e Motor de Cálculo (exec-plan `completed/0005`, `PlugV2CalculationEngine`, `ICalculationEngineResolver`, `BrokerageInsurerEnablement`).

## Objetivo

O lado da fonte do catálogo e o job de importação: `ImportedModality`, `ImportedGroup`, `ModalityMapping`; importação periódica pelo Motor de Cálculo resolvido pela Habilitação (RN-031), upsert idempotente por identificador de origem (RN-030), mapeamento automático **só por identificador do motor**, dentro do mesmo ramo (RN-032), desativação do que sumiu e resiliência por Seguradora (RN-035). Semelhança automática NÃO entra (OPEN-08). Mapa/Fila e disponibilidade derivada são fatia 3.

## Contrato PlugV2 observado (dev, 2026-07-22)

`POST /GetGroupAndModalities` com `{ BrokerCnpj }` (chave `application-key-v2` por Habilitação) retorna `BaseResponse` com `Response: GroupsAndModalities[]` — **uma entrada por Seguradora habilitada da corretora** (o Anexo A previa uma chamada por Seguradora; o contrato observado retorna todas — Anexo A autoriza adaptar). Campos usados: `Insurance.{Id,Name,InsuranceUniqueId}`, `IsSuccess` (resiliência por Seguradora, RN-035), `GlobalModalities[].{Id,Name}` (identificador do motor), `Modalities[].{ModalityUniqueId (reencontro), Name, BranchCode (75=Público,76=Privado), ModalityGroupUniqueId/Name/Type, e parâmetros comerciais}`. A Seguradora casa por `InsuranceUniqueId` = `Insurer.ReferenceExternalId`.

## Tarefas

- [x] Migration `V20260722010830__criar-tabelas-modalidade-importada.sql`: `dbo.ImportedGroups`, `dbo.ImportedModalities`, `dbo.ModalityMappings` (guards, FKs, únicos por (InsurerId,SourceId); único filtrado `WHERE Status='Confirmed'`; auditoria; RN + ADR-058). Aplicada no docker.
- [x] Core: enums `EImportedModalityStatus`, `ESuretyBranch`, `EModalityMappingStatus`, `EMappingEstablishment`; entidades ricas `ImportedGroup`, `ImportedModality` (upsert pela fonte, reativa ao reaparecer, desativa idempotente), `ModalityMapping` (`CreateByIdentifier` → Confirmed).
- [x] Core: `IImportedGroupRepository`, `IImportedModalityRepository`, `IModalityMappingRepository`; `ListActiveForImportAsync` em `IBrokerageInsurerEnablementRepository` (CNPJ da Corretora + Referência de origem da Seguradora + conexão).
- [x] Infra.Data: mappings 1:1, repositórios, DbSets, DI.
- [x] Integration: `GetGroupAndModalitiesAsync` em `ICalculationEngine`; provider PlugV2 (HttpClient com base URL por Habilitação + resiliência ADR-044, client resolvido sob demanda); ACL PlugV2 → contrato do motor (ADR-045); `PlugV2CalculationEngine` implementa a operação.
- [x] Application: `ModalityImporter` — por Corretora resolve o motor pela Habilitação, casa Seguradoras por `ReferenceExternalId`, upsert (cache in-batch de grupos + dedup de origem), mapeia por identificador no mesmo ramo (RN-032), desativa o que sumiu, isola falha por Corretora/Seguradora (`IsSuccess`, RN-035), dedup por Seguradora.
- [x] Functions: `ModalityImportFunction` (TimerTrigger diário 03:00 UTC, OPEN-10) compõe a DI do host e roda o importer. + Api `POST /modality-imports/run` (Administrador do Sistema) para disparo manual.
- [~] Rastreio: resumo por execução (`ModalityImportSummary`: processadas/sucesso/falha + motivos) retornado e logado. Tabela dedicada `ModalityImportRun` **adiada** (observabilidade via log/summary por ora).
- [x] Testes `[Trait("RuleId","RN-030/031/032/035")]`: entidades (7), ACL do PlugV2 com amostra do contrato real (3), importer contra motor fake (5: upsert+map por identificador, grupo compartilhado 1x, sem-map, falha isolada, desativação).
- [x] `dotnet build`/`dotnet test` verdes, `check-harness.py` ok, migration aplicada localmente.
- [x] Teste ao vivo: importação real contra a corretora Bravo (CNPJ 34060267000196) no PlugV2 dev — Essor (49) + Excelsior (25) = 74 modalidades importadas, ramos e grupos corretos, verificado no banco.

## Critérios de aceite

- Importação bem-sucedida cria/atualiza Modalidades Importadas por identificador de origem, preservando identidade e mapeamento (RN-030); nome/ramo/parâmetros refletem a fonte, sem edição à mão.
- Mapeamento automático só confirma quando o identificador do motor já aponta para uma Modalidade, no mesmo ramo (RN-032); o resto fica sem mapeamento confirmado (Fila, fatia 3). Semelhança não roda (OPEN-08).
- Importada Ativa ausente numa importação bem-sucedida da Seguradora vira Inativa; reaparecendo, reativa (RN-035). Falha/`IsSuccess=false` de uma Seguradora não desativa nada dela nem afeta as demais.
- A importação nunca cria Modalidade (lado Smart) — só o lado importado e mapeamentos por identificador (RN-032/ADR-060).
- Gates verdes; migration aplicada; teste ao vivo com Bravo dev evidenciado.

## Evidências

- **Backend** (2026-07-22): `dotnet build SmartInsure.slnx` 0 erros; `dotnet test` **316/316** (fatia 2 somou 15: entidades importadas 7, ACL do PlugV2 3, importer 5). `check-harness.py` → `harness ok`.
- **Migration**: Flyway aplicou `V20260722010830 - criar-tabelas-modalidade-importada` no docker (schema em v20260722010830).
- **Contrato**: `docs/generated/openapi.json` regenerado — inclui `POST /api/v1/modality-imports/run`.
- **Contrato PlugV2 observado**: probe dev com a chave da Bravo confirmou o formato (BranchCode 75/76, `IsSuccess` por Seguradora, `GlobalModalities[].Id`, `ModalityUniqueId`) — construído contra o contrato real (ADR-045).
- **Teste ao vivo** (PlugV2 dev, corretora Bravo 34060267000196): `POST /modality-imports/run` → `{processadas:2, sucesso:2, falha:0}`. No banco: **Essor 49 modalidades / 7 grupos** (38 público, 11 privado), **Excelsior 25 / 8 grupos** (17/8) = **74 modalidades**, parâmetros comerciais preservados como JSON, ramos corretos. **0 mapeamentos confirmados** — correto: sem Modalidade curada ainda, tudo aguarda a Fila (fatia 3). Bug de grupo duplicado (índice único) encontrado e corrigido (cache in-batch) com teste de regressão.
- **Commits** (branch `ab-0002-job-importar-modalidades`, sem PR): backend `9479f9d`(exec-plan), `cd46b34`(entidades), `06c3f41`(persistência), `ce5c366`(PlugV2+ACL), `87e7f89`(importer), `f1dfb24`(timer+endpoint), + fix/contrato; dbmigration `V20260722010830`.
- **Segredo**: a PlugKey da Bravo vive só no scratchpad local e nos parâmetros da Habilitação **no banco docker** (dev-seed) — nunca em arquivo versionado (SECURITY.md).
- Pendências: ratificação da PO; fatia 3 (Mapa/Fila + disponibilidade derivada); PRs; (opcional) tabela `ModalityImportRun`; semelhança automática (OPEN-08).
