# Exec-plan 0016 — Grupo de Cotação imutável: fork por mudança de dado-base — RN-050/051/060/061 — AB# pendente (slug `grupo-cotacao-imutavel`)

Status: **ativo** (2026-07-31). Design **ratificado** por Diego Estácho no lugar da PO (registrar confirmação da PO): Grupo imutável nos dados-base **a partir da 1ª Cotação**; **qualquer dado-base** dispara **fork** (Grupo novo); Grupos resultantes **independentes** (sem vínculo de origem → sem migration). Atividade **cross-repo**, uma branch `grupo-cotacao-imutavel` (backend/front de `main`). Refinamento em `.grill/grupo-cotacao-imutavel.md`.

Contexto obrigatório (ler antes de executar): `AGENTS.md`; RNs `grupo-de-cotacao.md` (RN-050/051), `cotacao.md` (RN-060/061); ADR-034 (FK navigation-less Restrict), ADR-036 (UoW concluída no use-case), ADR-004 (nome estável). **Origem:** observação ao vivo — o front reusava/mutava o MESMO Grupo ao mudar o Tomador (PUT, mesmo id), misturando/perdendo as Cotações; com a **Listagem** (livro de Cotações), recotar substituindo corrompe o registro. Um Grupo => um pedido (1 tomador/segurado/modalidade/IS/vigência).

## Objetivo

Tornar o Grupo de Cotação **imutável nos dados-base a partir da 1ª Cotação obtida**: mudar qualquer dado-base (Tomador, Segurado, escopo de Seguradoras, Modalidade, valor segurado, vigência, Coberturas Adicionais) num Grupo já cotado **cria um Grupo novo** (fork), preservando intactos o anterior e suas Cotações (inclusive a escolhida). Enquanto **sem** Cotações, segue editando no lugar (RN-051, evita rascunhos órfãos). O **servidor recusa** a edição de Grupo cotado (fail-closed). **Sem mudança de schema** (Grupos independentes). Sem mudança de contrato (nenhum endpoint/DTO novo).

## Tarefas (branch `grupo-cotacao-imutavel`)

- [x] **T1 — RN.** Reescrever **RN-060** (de "recálculo/invalidação no mesmo grupo" → "imutabilidade + fork"); ajustar **RN-050/051** (edita no lugar só enquanto sem Cotações; servidor recusa update de Grupo cotado; lista de gatilhos reconciliada — passa a incluir Tomador/Segurado); ajustar **RN-061** (Cotação não é mais invalidada por edição — mudança forka).
- [x] **T2 — Backend enforce (RN-060, fail-closed).** `IQuotationRepository.ExistsForGroupAsync` + impl (`AnyAsync`/EXISTS); `UpdateQuotationGroupUseCase` recusa (`ConflictException`) o update de Grupo que já tem Cotações. Teste `[Trait("RuleId","RN-060")]` — `Execute_DeveRecusar_QuandoGrupoJaTemCotacoes`.
- [x] **T3 — Frontend fork (RN-060).** `useQuotationGroupWizardStore.resetQuotationsForFork` (troca para o Grupo novo e zera cotação/assinatura/seleção/minuta, preservando os dados-base já alterados); `Wizard.vue` — ao sair do risco, se `signatureChanged` (Grupo cotado + dado-base mudou) abre **confirmação bloqueante** ("iniciar nova cotação com os dados alterados?") e cria Grupo novo (POST sem id) via `persistGroup(fork=true)`; aviso trocado de "recalcular" → "nova cotação". Comentário do `useQuotationGroups` atualizado.
- [x] **T4 — Gates.** Backend `dotnet test` (novo teste incluído). Front lint/typecheck/vitest. **Nota:** `check-harness` do backend fica **vermelho por colisão PRÉ-EXISTENTE da main** (`RN-062/063` minuta×perfis) — resolvida no PR aberto da listagem, **NÃO** por esta atividade; rebasear após o merge da listagem.

## Critérios de aceite

- **CA-01** (RN-051) — Grupo **sem** Cotações: voltar e mudar dado atualiza **no lugar** (mesmo id).
- **CA-02** (RN-060) — Grupo **com** Cotações: mudar dado-base pede **confirmação bloqueante** e cria um **Grupo NOVO** (id novo), preservando o anterior + Cotações; recusar não muda nada.
- **CA-03** (RN-060, servidor) — PUT em Grupo com Cotações → **409** (fail-closed), independente do front.
- **CA-04** (RN-061) — Cotação **não** é invalidada por edição; só o fork gera outra.
- Gates verdes (exceto a colisão `RN-062/063` **pré-existente**, herdada da main).

## Evidências

- Backend: `dotnet test --filter QuotationGroup` → **12 passed** (inclui `Execute_DeveRecusar_QuandoGrupoJaTemCotacoes`, `RN-060`).
- Front: lint/typecheck/vitest (resultado no PR).
- Contrato inalterado (nenhum endpoint/DTO novo) → **sem regen de openapi**.
