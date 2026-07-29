# Exec-plan 0011 — Coberturas Adicionais: curadoria e importação (RN-040..RN-046)

Status: **concluído** (backend) — 2026-07-23, AB#0003. RN-040..RN-046 aprovadas; implementação e teste ao vivo concluídos (ver Evidências). Backend-only; a tela de curadoria no front é fatia seguinte linkada pelo mesmo AB#0003. Pendente de publicação: `openapi.json` (derivado, no CI).

Contexto obrigatório (ler antes de executar): `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, `docs/PLANS.md`; RNs em `docs/product-specs/regras-de-negocio/coberturas-adicionais.md` (RN-040..RN-046); glossário (Cobertura Adicional, Cobertura Adicional Importada — propostos); `open-decisions.md` (OPEN-16; OPEN-09/OPEN-10 por analogia). Base/padrão a espelhar: Catálogo de Modalidades ([ADR-061](../../adr/061-modalidade-derivada-da-global-modality.md), RN-032..RN-039) — Modalidade canônica curada + Modalidade Importada + Fila; Motor de Cálculo (`completed/0005`).

## Objetivo

Duas faces do catálogo de Coberturas Adicionais e a curadoria que as liga:

- **Cobertura Adicional** (canônica): item do Smart, nome único, curado pelo Administrador do Sistema (criar/editar/ativar/inativar), o que o corretor vê na cotação (RN-040/RN-046).
- **Cobertura Adicional Importada**: a versão de cada Seguradora, por Modalidade Importada, trazida por importação exatamente como a OnPoint expõe (RN-041); mantida por importação periódica/sob demanda com reconciliação e resiliência (RN-042/RN-044/RN-045).
- **Vínculo manual** Importada→Canônica pelo Administrador, com a Fila de pendências (importadas Ativas sem vínculo) evidente; ignorar/reativar; vínculo preservado na reimportação (RN-043). A importação nunca cria nem vincula a canônica.

## Contrato PlugV2 (verificado no dev, ADR-045)

`POST {baseUrl}/GetAdditionalCoverages`, header `application-key-v2` — uma chamada por Modalidade Importada (`BrokerCnpj`, `InsuranceUniqueId`, `ModalityName`, `ModalityGroupType`). Envelope `{ StatusCode, Response.AdditionalCoverages[], HasError, Errors }`, item `{ BranchName, BranchCode, AdditionalCoverages{ Name, UniqueId, InsuredAmountCalculationType, AllowManualEdit } }`. **BranchCode observado = "75"/"76"** (Público/Privado); a ACL mapeia 75/76 e as formas SUSEP 0775/0776. `HasError`/corpo nulo = falha daquela modalidade (RN-045).

## Tarefas

- [x] Core: `AdditionalCoverage` (canônica: Create/Rename/Activate/Deactivate, nome único) e `ImportedAdditionalCoverage` (reflete a fonte; LinkTo/Unlink/Ignore/Restore; UpdateFromSource preserva o vínculo); enums de status. Testes de entidade `[Trait("RuleId","RN-040/041/043/044")]`.
- [x] Persistência: mappings (AdditionalCoverages nome único; ImportedAdditionalCoverages único (ImportedModalityId, Name), FK à canônica nulável, FK à Modalidade Importada), repositórios (upsert; ativas por modalidade; Fila de pendências; vinculadas; catálogo), DbSets, DI.
- [x] Integração: contrato `ImportedAdditionalCoverageResult/Data`; ACL `GetAdditionalCoverages` (erro/nulo → falha; ramo → Público/Privado; sem-ramo descartada) com teste sobre amostra real; client HTTP por Habilitação; `PlugV2CalculationEngine`.
- [x] Importação: `AdditionalCoverageImporter` — por Seguradora Ativa/habilitada (dedup, OPEN-09), upsert da importada por (modalidade+nome), ramo confirma, reconciliação desativa ausentes só em sucesso e preserva vínculo, isola falha por modalidade. 6 testes contra motor fake.
- [x] Curadoria (Administrador do Sistema): use cases + endpoints — CRUD da canônica (`POST/PUT /additional-coverages`, `/{id}/activate|inactivate`), `GET /additional-coverages/map` (catálogo + Fila); `POST /imported-additional-coverages/{id}/link|unlink|ignore|restore`.
- [x] Importação agendada (Function TimerTrigger, cadência configurável) + disparo sob demanda (`POST /additional-coverage-imports/run`, Administrador do Sistema).
- [x] Migration (repo `smartinsure-dbmigration`): `V20260723155211__criar-tabelas-coberturas-adicionais.sql` (duas tabelas). Aplicada no docker.
- [x] Gates: `dotnet build`, `dotnet test` (365), `check-harness`. [ ] `openapi.json` (CI — regen local bloqueada por segredos de dev).

## Critérios de aceite

- CA-01: cobertura Ativa na OnPoint para uma Modalidade Importada processável vira Cobertura Adicional Importada Ativa, pendente de mapeamento até o vínculo manual (RN-041/RN-043).
- CA-02: importada ausente na OnPoint fica Inativa após o próximo ciclo bem-sucedido, mantendo o vínculo (RN-044).
- CA-03: falha de uma modalidade não desativa suas importadas nem interrompe as demais (RN-045).
- CA-04: nome duplicado (após normalização) na mesma Modalidade Importada é a mesma importada; nome canônico é único (RN-040/RN-041).
- CA-05: o Administrador cria/edita/ativa/inativa a canônica, vincula/desvincula/ignora importadas e vê a Fila; a canônica é oferecida quando tem ≥1 importada Ativa vinculada (RN-046).

## Evidências

- **Build/Testes** (2026-07-23): `dotnet build` 0 erros; `dotnet test` **365/365** (19 de coberturas: canônica 3, importada 6, ACL 4, importer 6), `[Trait RuleId RN-040..046]`; NetArchTest verde. `check-harness` ok.
- **Migration**: Flyway aplicou a cadeia completa (18) em banco docker limpo, incluindo `V20260723155211 - criar-tabelas-coberturas-adicionais`; `AdditionalCoverages` (nome único) e `ImportedAdditionalCoverages` (único (ImportedModalityId, Name), FKs) verificadas.
- **Contrato PlugV2 (probe dev)**: envelope e item confirmados; BranchCode real `75`/`76`.
- **Teste ao vivo (PlugV2 dev, Bravo 34060267000196)**: modalidades 2 Seguradoras/74; coberturas **74 processadas, 74 sucesso, 0 falha** → **43 Coberturas Adicionais Importadas Ativas, todas pendentes de mapeamento; catálogo canônico vazio** (importação não cria canônica), 0 duplicatas. Curadoria validada: ao vincular a canônica "Multa" às 17 importadas homônimas, pendentes caíram para 26, vínculos ativos 17, canônica ofertável 1 (RN-046). Harness de execução foi scratch (lê a DB local; sem segredo no arquivo) — não commitado.
- **Segredo**: chave da Bravo só no scratchpad local e no `ConnectionParameters` no banco docker — nunca versionada (SECURITY.md).
- **Pendências**: front da curadoria (fatia linkada); `openapi.json` no CI; ratificação formal da PO.
