# Exec-plan 0017 — Consulta de Crédito camada 2 (design homologado)

Status: ativo — evolução cross-repo da Consulta de Crédito (entregue em 0007) para a fidelidade
total do design homologado (`prototipos/consulta de credito/handoff-consulta-credito/01-consulta-credito.md`):
um único componente com duas entradas (página `/consulta-credito` e modal do passo 1 da cotação),
tempo de resposta por Seguradora, busca de Tomador enriquecida e o quadro consolidado com colunas fixas.

Contexto obrigatório (ler antes de executar): AGENTS.md, ARCHITECTURE.md, docs/BACKEND.md,
docs/SECURITY.md, RNs da jornada Consulta de Crédito (`docs/product-specs/regras-de-negocio/consulta-de-credito.md`),
RN-027/028 (Nomeação de Tomador), glossário (Consulta de Crédito/`CreditInquiry`, Limite de Crédito/`CreditLimit`,
Nomeação/`PolicyHolderAppointment`), open-decisions (OPEN-08 — validade e gestão manual de limite FORA desta entrega).

## Decisões de escopo (ratificadas com a PO na abertura — 2026-08-04)

- **Status por Seguradora fica em 2 estados** (Aprovado/Indisponível). O Motor de Cálculo (PlugV2
  `GetPolicyHolderLimitsAndRates`) não sinaliza um terceiro estado ("em análise"); não se inventa.
- **"Porte" do Tomador não entra** no card de candidato: não há fonte persistida (o `CompanySize`
  do bureau é transitório, só no cadastro). Fica registrado como melhoria futura.
- **Validade do limite: coluna presente, valor ausente (—)**, como o OPEN-08 sanciona. Investigação
  no repositório do PlugV2 (OnPoint-Backend) confirmou que a validade só existe no fluxo manual da
  assessoria (`AgentUpdateExpirationDate…`), fora do retorno automático do motor e fora desta entrega.
- **Taxa fiscal do judicial não é campo novo**: o motor já devolve `GARANTIA_JUDICIAL` e
  `GARANTIA_JUDICIAL_FISCAL` como grupos separados (cada um com sua taxa); a coluna Judicial compõe
  os dois no front. (Consistente com o `JudicialFiscalRate` descartado no 0007.)
- **Tempo de resposta por Seguradora é persistido** (medição própria do fan-out): habilita o histórico
  completo (RN-031) — exige +1 coluna e migration na `develop`.

## Objetivo

1. **Backend (contrato primeiro)** — expor `ResponseTimeMs` por Seguradora no resultado da consulta
   (medido no fan-out, persistido) e enriquecer a busca de Tomador com cidade/UF e a flag
   "já é tomador da corretora ativa" (derivada de Nomeação Vigente — RN-027/028).
2. **DBMigration (`develop`)** — persistir `ResponseTimeMs` no resultado da consulta (histórico imutável).
3. **Frontend** — reescrever a tela como um componente com duas entradas (`mode: page | embed`),
   com a fidelidade do design (6 estados, KPIs, quadro consolidado com `table-layout: fixed`, colunas
   fixas por `GroupType`, taxa fiscal composta, utilizado com barra, validade ausente), o modal no
   passo 1 da cotação ("Ver limites e taxas") e a lista de cards no mobile (alvos ≥ 44px).

## Tarefas

- [~] RN: refinar RN-029/031 (tempo de resposta) e RN-104 (busca de Tomador enriquecida) — docs atualizados; **aprovação da PO pendente**
- [x] open-decisions: anexar ao OPEN-08 a evidência do PlugV2 (validade só no fluxo manual/assessoria)
- [x] DBMigration (`develop`): coluna `ResponseTimeMs` em `CreditInquiryResults` (nullable), migration Flyway numerada cirurgicamente
- [x] Core: `CreditInquiryResult.ResponseTimeMs`; `Available`/`Unavailable` recebem o tempo medido
- [x] Application: mede o tempo por chamada (Stopwatch) e o propaga; `BuildResponse` expõe `ResponseTimeMs`
- [x] Application/Api: busca de Tomador (`ListPolicyHolders`) recebe a Corretora ativa e devolve `City`, `StateCode`, `IsAppointedToBrokerage`
- [x] Infra.Data: mapping EF do novo campo; enriquecimento da query de busca (cidade do endereço principal; join de Nomeação Vigente)
- [x] Testes xUnit `[Trait("RuleId", "RN-029|031|104")]` (tempo medido, isolamento RN-030, flag de nomeação vigente/encerrada)
- [x] Contrato: regenerar `docs/generated/openapi.json`
- [x] Frontend: types gerados; componente `mode: page|embed`; modal no passo 1; mobile; testes Vitest + E2E (jornada reescrita, 5/5); evidência desktop/mobile capturada e medida
- [x] Verificação: gates dos três repos + `python scripts/check-harness.py` (todos verdes)
- [ ] PRs linkados (mesmo AB#, pendente)

## Critérios de aceite

- `dotnet build` e `dotnet test` verdes (cobertura ≥ 80% nas classes novas/alteradas)
- Front: `pnpm lint`, `pnpm typecheck`, `pnpm test`, `pnpm build` verdes; E2E da jornada
- `python scripts/check-harness.py` verde (backend e frontend)
- Tempo de resposta por Seguradora medido no fan-out e persistido; visível na consulta e no histórico (RN-031)
- Falha isolada (RN-030): a Seguradora que não responde fica com `ResponseTimeMs` ausente (null) e não derruba as demais; o tempo medido é gravado apenas nas respostas efetivas (Available) — ratificado com a PO em 2026-08-04
- Busca de Tomador devolve cidade/UF e a flag "já é tomador da corretora ativa" só quando há Nomeação Vigente com a Corretora (RN-027/028); nunca inventada
- Um componente, duas entradas: sem duplicação de código entre `page` e `embed`
- Validade apresentada como ausente (OPEN-08); "porte" ausente; status em 2 estados — sem inventar dado

## Evidências

Gates (worktree `consulta-credito-v2`, retomada 2026-08-04):

- Backend: `dotnet build` verde; `dotnet test` **663/663** verde; `check-harness.py` verde.
- Frontend: `pnpm install`/`typecheck`/`lint` verdes; `vitest` **353/353** verde; `nuxt build` verde; `check-harness.py` verde.
- DBMigration: migration `V20260804120000__adicionar-response-time-em-credit-inquiry-results.sql` (idempotente, `COL_LENGTH` guard), numerada após a última da `develop`.
- E2E (Playwright, projeto `ui`): `consulta-credito.spec.ts` reescrito para o novo `CreditInquiryPanel` (RN-029/030/031) — **5/5 verde**.
- Evidência visual (ambiente local subido, login real): campos de busca medidos em `rgb(248,250,252)` (#F8FAFC) nas 3 telas (Consulta de Crédito, Cotações, Corretoras); screenshots desktop+mobile.
- Fix de layout no desktop: campo Tomador era esmagado por nome longo de Corretora (flex no `.v-input` interno, não no filho-flex) → campos envolvidos em `div`s (Corretora=300px trunca, Tomador=606px).
- Fix de regressão RN-029: o rewrite havia perdido a validação client-side do dígito do CNPJ; restaurada em `submitInquiry` com `isValidCnpj` ("CNPJ inválido" antes de qualquer consulta).

Notas da retomada:

- Fix de fixture no teste `credit-inquiry-helpers.spec.ts` (usava `?? 820`, engolia o `null` explícito de Indisponível).
- Renome de identificadores pt-BR → inglês nos componentes novos (ADR-058): `selectedTomador`→`selectedPolicyHolder`, `novaConsulta`→`startNewInquiry`, `consultar`→`submitInquiry`, `reconsultar`→`retryInquiry`; props `corretora/tomador…`→`brokerage/policyHolder…`.
- **Ponto aberto p/ PO — tempo de resposta em falha:** o critério de aceite diz "falha isolada (RN-030) preserva o tempo medido até a falha", mas a implementação (e os testes, entity e migration) gravam `ResponseTimeMs = null` quando a Seguradora não responde (indisponibilidade/falha). Decisão a ratificar: manter `null` (ajustar a redação do critério) ou passar a gravar o tempo-até-a-falha.
- **DS (fora do escopo original, PR à parte):** padrão "input de busca = fundo cinza" promovido ao kit (`.si-field--search` no `skin.css`), aplicado em `cotacoes` (migrado do hack local), `corretoras` e no campo da Consulta de Crédito.
