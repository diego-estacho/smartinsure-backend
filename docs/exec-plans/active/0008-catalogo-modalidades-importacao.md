# Exec-plan 0008 — Catálogo de Modalidades, Fatia 2: Importação (RN-030, RN-031, RN-032, RN-035)

Status: ativo — AB#0002 (`ab-0002-job-importar-modalidades`). Em execução.
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/adr/060-catalogo-modalidades-dois-mundos.md`, RNs em `docs/product-specs/regras-de-negocio/modalidades.md` (RN-030/031/032/035), spec de produto (Anexo A — contrato PlugV2), OPEN-08 (semelhança fora de escopo), OPEN-09 (credencial multi-corretora), OPEN-10 (cadência). Base: fatia 1 (exec-plan 0007, entidades `Modality`/`ModalityGroup`) e Motor de Cálculo (exec-plan `completed/0005`, `PlugV2CalculationEngine`, `ICalculationEngineResolver`, `BrokerageInsurerEnablement`).

## Objetivo

O lado da fonte do catálogo e o job de importação: `ImportedModality`, `ImportedGroup`, `ModalityMapping`; importação periódica pelo Motor de Cálculo resolvido pela Habilitação (RN-031), upsert idempotente por identificador de origem (RN-030), mapeamento automático **só por identificador do motor**, dentro do mesmo ramo (RN-032), desativação do que sumiu e resiliência por Seguradora (RN-035). Semelhança automática NÃO entra (OPEN-08). Mapa/Fila e disponibilidade derivada são fatia 3.

## Contrato PlugV2 observado (dev, 2026-07-22)

`POST /GetGroupAndModalities` com `{ BrokerCnpj }` (chave `application-key-v2` por Habilitação) retorna `BaseResponse` com `Response: GroupsAndModalities[]` — **uma entrada por Seguradora habilitada da corretora** (o Anexo A previa uma chamada por Seguradora; o contrato observado retorna todas — Anexo A autoriza adaptar). Campos usados: `Insurance.{Id,Name,InsuranceUniqueId}`, `IsSuccess` (resiliência por Seguradora, RN-035), `GlobalModalities[].{Id,Name}` (identificador do motor), `Modalities[].{ModalityUniqueId (reencontro), Name, BranchCode (75=Público,76=Privado), ModalityGroupUniqueId/Name/Type, e parâmetros comerciais}`. A Seguradora casa por `InsuranceUniqueId` = `Insurer.ReferenceExternalId`.

## Tarefas

- [ ] Migration `V20260722010830__criar-tabelas-modalidade-importada.sql`: `dbo.ImportedGroups`, `dbo.ImportedModalities`, `dbo.ModalityMappings` (guards, FKs, únicos: (InsurerId,SourceId) nas importadas/grupos; único filtrado `WHERE Status='Confirmed'` por Importada em ModalityMappings; auditoria; header RN + ADR-058).
- [ ] Core: enums `EImportedModalityStatus`, `ESuretyBranch`, `EModalityMappingStatus`, `EMappingEstablishment`; entidades ricas `ImportedGroup`, `ImportedModality` (upsert pela fonte, reativação ao reaparecer, desativação idempotente), `ModalityMapping` (`CreateByIdentifier` → Confirmed).
- [ ] Core: `IImportedGroupRepository`, `IImportedModalityRepository`, `IModalityMappingRepository` (buscar por (InsurerId,SourceId), ativas por Seguradora, por EngineModalityId com mapeamento confirmado); método em `IBrokerageInsurerEnablementRepository` para listar Habilitações Ativas com CNPJ da Corretora + parâmetros de conexão.
- [ ] Infra.Data: mappings 1:1, repositórios, DbSets, DI.
- [ ] Integration: `GetGroupAndModalities` em `ICalculationEngine`; `IPlugV2Api` (Refit) + provider com base URL/kaader por Habilitação (ADR-046/044); ACL PlugV2 → contrato do motor (ADR-045); `PlugV2CalculationEngine` implementa a operação.
- [ ] Application: `ModalityImporter` (serviço) — por Corretora: chama o motor, casa Seguradoras por `ReferenceExternalId`, upsert grupos/modalidades importadas (RN-030), mapeia por identificador dentro do mesmo ramo (RN-032), desativa o que sumiu numa importação bem-sucedida, isola falha por Seguradora (RN-035, `IsSuccess`).
- [ ] Rastreio: `ModalityImportRun` (resumo por execução: início/fim, totais, falhas com motivo).
- [ ] Functions: timer trigger que compõe a DI (espelha `AddApiServices`) e roda o importer sobre as Habilitações Ativas (cadência configurável — OPEN-10).
- [ ] Testes `[Trait("RuleId","RN-030/031/032/035")]` (xUnit/NSubstitute/FluentAssertions): entidades, ACL do PlugV2 (com amostra real), importer contra `ICalculationEngine` fake (upsert, mapeamento por identificador, trava de ramo, desativação, falha isolada).
- [ ] `dotnet build`/`dotnet test` verdes, `check-harness.py` ok, migration aplicada localmente, cobertura das novas classes.
- [ ] Teste ao vivo: importação real contra a corretora Bravo (CNPJ 34060267000196) no PlugV2 dev — Essor/Excelsior importadas, verificado no banco.

## Critérios de aceite

- Importação bem-sucedida cria/atualiza Modalidades Importadas por identificador de origem, preservando identidade e mapeamento (RN-030); nome/ramo/parâmetros refletem a fonte, sem edição à mão.
- Mapeamento automático só confirma quando o identificador do motor já aponta para uma Modalidade, no mesmo ramo (RN-032); o resto fica sem mapeamento confirmado (Fila, fatia 3). Semelhança não roda (OPEN-08).
- Importada Ativa ausente numa importação bem-sucedida da Seguradora vira Inativa; reaparecendo, reativa (RN-035). Falha/`IsSuccess=false` de uma Seguradora não desativa nada dela nem afeta as demais.
- A importação nunca cria Modalidade (lado Smart) — só o lado importado e mapeamentos por identificador (RN-032/ADR-060).
- Gates verdes; migration aplicada; teste ao vivo com Bravo dev evidenciado.

## Evidências

- (a preencher) build/test, cobertura, check-harness, migration Flyway, importação ao vivo (contagens por Seguradora), commits.
