# Exec-plan 0017 — Cotação com Cobertura Adicional: enviar ao PlugV2 o que o corretor escolheu — RN-104/105/106 — AB#0007 (slug `ab-0007-cotacao-cobertura-adicional`)

Status: **ativo** (2026-08-04). Atividade **cross-repo, backend primeiro** (toca prêmio e contrato): worktrees `C:\wt\ab-0007\{smartinsure-backend,smartinsure-frontend,smartinsure-dbmigration}`, branch `ab-0007-cotacao-cobertura-adicional` nos três. RN-104/105/106 catalogadas como **proposta — aguardando aprovação da PO**; o código fica na branch até essa aprovação.

Contexto obrigatório (ler antes de executar): `AGENTS.md`; RNs `coberturas-adicionais.md` (RN-040..046, em especial **RN-046** — oferta pela canônica com disponibilidade derivada dos vínculos), `grupo-de-cotacao.md` (RN-050/051/102/**104**), `cotacao.md` (RN-056..063, **105**, **106**, RN-103); **ADR-103** (envio pelo nome da Importada — a decisão central, com a evidência do probe), ADR-064 (classificação do resultado), ADR-045/ADR-028 (a ACL não vaza modelo de fornecedor nem hospeda regra), ADR-041..043 (Flyway dono do schema, forward-only), ADR-029/030 (EntityBase, UUIDv7), ADR-031 (enum como string), ADR-034 (FK Restrict), **OPEN-22** (nome divergente por ramo) e **OPEN-16** (semântica do tipo de cálculo — mantém valor segurado por cobertura fora de escopo).

**Origem:** investigação de 2026-08-04. `QuotationRequestProcessor` enviava `AdditionalCoverages` **hardcoded vazio**, com `TODO(probe T14)`: toda Cotação era precificada só com a garantia principal, independentemente do que o corretor marcasse na etapa 3, e nada avisava o usuário. O Grupo guardava apenas dois booleanos provisórios (`IncludesPenaltyCoverage`/`IncludesLaborCoverage`), incapazes de representar o catálogo real.

## Objetivo

Fazer a Cotação chegar à Seguradora **com as Coberturas Adicionais que o corretor escolheu**. A escolha passa a ser relação com a Cobertura Adicional **canônica** (os dois booleanos saem do domínio e do contrato); no fan-out, um resolvedor na Application traduz canônica → **nome** da Importada daquela Seguradora/Modalidade (ADR-103) e devolve o que ela não oferece; cada Cotação registra a situação de cada cobertura (**Enviada** / **Não contemplada**) e a comparação sinaliza a lacuna, para que prêmios de escopos diferentes nunca sejam comparados sem aviso. Seguradora que não oferece a cobertura **é cotada mesmo assim, sem ela** (decisão de 2026-08-04).

## Tarefas (branch `ab-0007-cotacao-cobertura-adicional`)

- [x] **T1 — RNs e decisão aberta.** RN-104 (`grupo-de-cotacao.md`), RN-105/RN-106 (`cotacao.md`), OPEN-22 (`open-decisions.md`), marcador "Situação da Cobertura Adicional na Cotação" no glossário. **Gate:** aprovação da PO antes do merge.
- [x] **T2 — ADR-103.** Envio pelo nome da Importada, com a evidência do probe (GUID recusado com erro nomeando o valor; 400 derruba a Cotação inteira; dedup ignora IS e vigência).
- [x] **T3 — Este exec-plan.**
- [x] **T4 — Migrations (`smartinsure-dbmigration`).** Corrige o nome da canônica semeada com typo (`Trabalhista e Previdênciário` → `Previdenciária`); cria `QuotationGroupAdditionalCoverages` e `QuotationAdditionalCoverages`; converte os booleanos para a relação e **derruba as duas colunas** (falha alto se a canônica não existir — perder seleção afeta prêmio).
- [x] **T5+T9 — Domínio e contrato (atômico).** `EQuotationAdditionalCoverageStatus`, as duas entidades, `IQuotationAdditionalCoverageResolver` + records em Core; `QuotationGroup` sem booleanos e com a coleção; `Quotation.RecordAdditionalCoverages`; mappings e DbSets; e no **mesmo commit** request/response/use cases/endpoint com `additionalCoverageIds` (breaking).
- [x] **T6 — Consultas.** `ListForQuotationAsync` (por Seguradora/Modalidade/canônicas) e `ListAvailableForModalityAsync` (união simples por Corretora do Escopo ativo), com o **mesmo critério de derivação** para oferta e envio nunca divergirem.
- [x] **T7 — Resolvedor (TDD).** `|N|`=1 envia; `|N|`=0 não contempla; `|N|`>1 não contempla (OPEN-22); ramos com nome igual enviam uma vez; Grupo sem cobertura não consulta catálogo.
- [x] **T8 — Fan-out (TDD).** `QuotationRequestProcessor` envia os nomes resolvidos e grava a situação **antes** de acionar o motor, para o registro sobreviver a Indisponível (RN-058) e a falha de integração (RN-057). Fecha o `TODO(probe T14)`.
- [x] **T10 — Endpoint do corretor.** `GET /api/v1/modalities/{id}/additional-coverages`, autorização default (o `/additional-coverages/map` existente é restrito a Administrador e não serve).
- [x] **T11 — Leitura das Cotações.** `additionalCoverages: [{ additionalCoverageId, name, status, sentName? }]` — `name` é o da canônica; `sentName` só quando `Sent`.
- [ ] **T12 — Gates do backend + PR.**
- [ ] **T13..T16 — Front.** Composable do endpoint novo; store com `additionalCoverageIds` e **limpeza ao trocar Modalidade**; etapa 3 dinâmica (sai o par de checkboxes fixos); marcador `NotOffered` na comparação; gates. **Bloqueado até o merge do backend** — `openapi.json` não regenera local, é publicado no CI, e `pnpm types:gen` depende dele.

## Critérios de aceite

1. Cotação solicitada com cobertura escolhida chega à Seguradora com o **nome** da Importada em `AdditionalCoverages`; nunca com o `SourceUniqueId` (ADR-103).
2. Uma canônica escolhida contribui com **exatamente um** nome; a lista vai sem repetição; Grupo sem escolha envia `[]`.
3. Seguradora que não oferece a cobertura **é cotada**, e a Cotação registra a cobertura como **Não contemplada** — inclusive quando a Cotação resulta Indisponível ou falha na integração.
4. Nome divergente entre ramos da mesma Seguradora **não é enviado** e consta como Não contemplada (OPEN-22) — nunca se envia superset.
5. A etapa 3 oferece apenas canônicas Ativas com vínculo ativo na Modalidade escolhida, nas Seguradoras habilitadas da Corretora do Escopo ativo; trocar a Modalidade descarta a seleção.
6. Os dois booleanos saem do domínio, do banco e do contrato; Grupos existentes são convertidos sem perder seleção.
7. A comparação sinaliza cada cobertura não contemplada, por **nome estável** do status.
8. Gates verdes: `dotnet build SmartInsure.slnx`, `dotnet test` ≥80%, `check-harness.py` sem violação **nova**; front `lint`/`typecheck`/`test`/`build` e E2E da jornada.

## Evidências

- **Importação de Coberturas Adicionais rodada** (2026-08-04, banco local, Habilitações da Finn apontando para o gateway QA, `POST /additional-coverage-imports/run`): `modalitiesProcessed: 216, modalitiesSucceeded: 216, modalitiesFailed: 0`, 88s → **249 Coberturas Adicionais Importadas**, 25 nomes distintos, `SourceUniqueId` em **100%** das linhas, **0 vínculos** com a canônica (curadoria é manual — RN-043).
- **Probe T14 do `POST /Cotation`** (gateway QA, corretora Finn, modalidade Licitante, 2026-08-04):
  - `["b4e65794-032b-4210-bfc4-6eef35210833"]` (SourceUniqueId) → **HTTP 400** `"Atenção! Existem coberturas informadas na criação da cotação que não são suportadas: b4e65794-…"`
  - `["Multas"]` (nome da Importada) → **HTTP 200**, `ResponseStatus.Status = 5`
  - Cobertura não suportada **derruba a solicitação inteira**; o dedup do gateway ignora variação de IS e de vigência.
  - **Não verificado:** aplicação da cobertura via variação de prêmio — o tomador usado cai em análise de subscrição (prêmio `0,00`). Reconfirmar com tomador sem pendência em QA.
- **Backend (2026-08-04):** `dotnet build SmartInsure.slnx` → **0 erros** (50 warnings, todos pré-existentes). `dotnet test tests/SmartInsure.Tests` → **668 testes, 0 falhas** (`main` tinha 646 — **+22** nesta atividade). Testes novos carregam o ID da RN: RN-104 (5 na entidade + 5 no endpoint), RN-105/RN-106 (7 no resolvedor + 3 no processor + 2 na leitura do leque).
- **Cobertura das classes novas: 100%** — `QuotationAdditionalCoverageResolver`, `ListAvailableAdditionalCoveragesUseCase`, `QuotationAdditionalCoverage`, `QuotationGroupAdditionalCoverage` e os dois mappings.
- **Condições pré-existentes registradas (nenhuma criada por esta atividade):**
  1. `check-harness.py` já acusa, na `main`, **RN-062 e RN-063 duplicados** entre `cotacao.md` e `perfis-e-permissoes.md`. Não foi corrigido aqui — ID de RN nunca é reaproveitado, então resolver a duplicata é decisão do time/PO. Esta entrega **não adiciona violação nova**.
  2. **A cobertura global já está abaixo do gate de 80%:** `main` = **58,81%**; esta branch = **59,50%** (+0,69pp). O gate de CI, portanto, já falhava antes desta atividade. Precisa de uma frente própria de cobertura.
- [ ] Aprovação da PO das RN-104/105/106 (e ciência de OPEN-22) — **pendente**.
- [ ] Gates do front + screenshot da etapa 3 e do marcador na comparação — **pendente (T16)**.
- [ ] Curadoria mínima dos vínculos canônica↔importada para demonstrar a jornada ponta a ponta (hoje 0 vínculos; sem ela a etapa 3 lista vazio, que é correto por RN-046 mas indistinguível de bug) — **pendente**.
