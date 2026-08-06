# Exec-plan 0018 — Página de Detalhes da Cotação (Fatia 1, read-only — backend)

Status: **em andamento** (2026-08-04). Slug `detalhe-cotacao`, AB# pendente. Fatia 1 (read-only) da Página de Detalhes; **cancelamento** (eixo-2), **emissão**, **Documentos** e **followup** são fatias/demandas seguintes. Segue a Listagem de Cotações (PRs #41/#46 já mergeados). PR de front linkado pelo mesmo AB#.
Contexto obrigatório: `AGENTS.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, `docs/QUALITY_SCORE.md`, **RN-081** (Página de Detalhes), RN-077/RN-078 (listagem/situação), RN-058 (classificação), RN-064/RN-103 (Escopo ativo), glossário (Cotação, Grupo de Cotação, situação apresentada), [ADR-064](../../adr/064-classificacao-resultado-cotacao.md), ADR-038 (DTO de projeção), ADR-031 (nome estável), `.grill/detalhe-cotacao.md`.

## Objetivo

O passo-4 persiste `Quotation` por `QuotationGroup` e a listagem já lê o livro achatado, mas **não há leitura de uma Cotação isolada** para a tela de detalhe. Esta fatia entrega o **`GET /quotations/{id}`** — leitura read-only de uma Cotação da Corretora do **Escopo ativo**, com os dados do pedido, comissão em valor (persistida), Coberturas Adicionais contempladas e uma **cronologia mínima composta no servidor** a partir de fatos reais — e publica o contrato para o front consumir.

## Escopo e não-escopo

- **No escopo:**
  - `GET /quotations/{id}` — detalhe de **uma** `Quotation` resolvida pela **identidade** (`quotationId`, guid), nunca pelo número (`ProposalNumber` é nullable e é só exibição). Projeção juntando o pai (`QuotationGroup`) + `Quotation` + `Insurer` + `Modality` + `Person` tomador/segurado (ADR-038).
  - **Campos:** número (`ProposalNumber`, vazio quando ausente), Tomador + **CNPJ** (`Person.documentNumber`), Segurado + **CNPJ**, Seguradora (+logo), Modalidade, importância segurada, prêmio, **comissão % e comissão em valor** (`CommissionValue`, persistida), **Coberturas Adicionais contempladas** (RN-106), vigência (início/fim), criada em, resultado (**nome estável** → situação apresentada RN-078), **`requiresCcg`/`ccgSigned`** (indicador ortogonal, RN-058).
  - **Cronologia mínima (`timeline[]`) composta no servidor** a partir de fatos que a plataforma conhece: Cotação criada (data do Grupo/Cotação), Cotação obtida da Seguradora (`ObtainedAt`) e, quando `RequiresCcg`, o marco de exigência de CCG. **Sem entidade de log nova.** Cada item: tipo/nome estável, rótulo, data. Ordem mais-recente-primeiro.
  - **Visibilidade/autorização (RN-064/RN-081):** só Cotações da Corretora do Escopo ativo (`ICurrentUserAccessor.ActiveBrokerageId` via `QuotationGroup.BrokerageId`). Fora do escopo (ou id inexistente) → **404** (não revela existência). Sem Escopo de Corretora → recusa (fail-closed, SECURITY.md).
- **Fora do escopo (com motivo):**
  - **Tela (front):** exec-plan próprio no repo do front (`0017-cotacao-detalhe.md`); esta fatia só publica o contrato.
  - **`objeto` (texto livre) e `propostaValidaAte`:** não modelados no domínio — não entram (pontos abertos; decisão própria). Nada de valor inventado.
  - **Abas Documentos e Follow-up, cenário `subscricao`:** subsistemas inexistentes (entidades de documento/mensagem) — fatias próprias, backend-first.
  - **Cancelamento (eixo-2), Emissão:** endpoints de escrita — fatias próprias. Nada de POST nesta fatia.
  - **Log de eventos durável:** a cronologia aqui é derivada dos fatos existentes; o log real nasce quando as ações (anexo/mensagem/cancel/emit) começarem a gerar eventos.

## Tarefas

- [x] DTO de projeção `QuotationDetailDto` (+ `QuotationDetailCoverageDto`) em `Core/Abstractions/Repositories/Dtos/QuotationDtos.cs` (join `Quotation`×`QuotationGroup`×`Insurer`×`Modality`×`Person` tomador/segurado + Coberturas Adicionais canônicas, ADR-038).
- [x] `IQuotationRepository.GetDetailAsync(quotationId, brokerageId)` — `AsNoTracking`, projeção, **escopado por `BrokerageId`**, mesma inclusão do livro (Obtained-provider); retorna nulo fora do escopo/inexistente.
- [x] `GetQuotationDetailUseCase` + Request (id + Escopo ativo) + Response (`QuotationDetailResponse` + `Timeline[]` + `QuotationTimelineEventTypes`) + Interface. Resultado/cobertura por **nome estável** (ADR-031). Cronologia composta no use case (criada/obtida/CCG).
- [x] `QuotationBookEndpoint`: `GET /{id:guid}` autenticado (rota `quotations`, junto do livro); **404** quando o detalhe é nulo.
- [x] Testes `[Trait("RuleId","RN-081")]` (6): sem Escopo → Forbidden; nulo → 404; mapeamento + resultado por nome estável + número vazio + comissão persistida; cronologia sem CCG (obtida→criada, mais recente 1º); cronologia com CCG (CcgRequired ancorado na obtenção); coberturas por nome estável.
- [x] `dotnet build` (0 erros) + `dotnet test --filter GetQuotationDetail` (6/6) + `check-harness.py` (ok).
- [ ] **`docs/generated/openapi.json`**: derivado pelo **CI** no merge do backend (não sobrescrever à mão — CRLF/CI; memória `regen-openapi-local-crlf`). Front regenera types do contrato do CI; localmente, types do OpenAPI da API local desta worktree (padrão do exec-plan 0017).

## Critérios de aceite

- `GET /quotations/{id}` devolve o detalhe da Cotação **da Corretora do Escopo ativo**, resolvido pela identidade (guid); Cotação de outra Corretora ou id inexistente → **404** idêntico (não revela existência).
- Traz Tomador/Segurado com CNPJ, Seguradora, Modalidade, IS, prêmio, comissão % e **em valor (persistida)**, Coberturas Adicionais contempladas, vigência, criada em, número (ou vazio), resultado por **nome estável** e os indicadores `requiresCcg`/`ccgSigned`.
- A `timeline[]` contém **apenas** marcos reais (criada, obtida, CCG quando exigido), ordem mais-recente-primeiro; nenhum evento inventado.
- Requisição sem Escopo de Corretora é recusada (fail-closed). `objeto` e `propostaValidaAte` **não** aparecem. **Sem migration**; nenhuma escrita.

## Evidências

- `dotnet build` (worktree, `origin/main` `e2c1d48`): **0 Errors**, 107 warnings (pré-existentes: CS86xx em testes + feed gclaims morto — `nuget-gclaims-feed-morto`).
- `dotnet test --filter "FullyQualifiedName~GetQuotationDetail"`: **Passed 6, Failed 0**.
- `python scripts/check-harness.py`: **harness ok**.
- Contrato: **`GET /api/v1/quotations/{id}` → `QuotationDetailResponse`** (identidade+número, tomador/segurado+CNPJ, seguradora, modalidade, IS, prêmio, comissão %+R$ persistida, vigência, criada em, `result` por nome estável, `requiresCcg`/`ccgSigned`, `additionalCoverages[]` por nome estável, `timeline[]`). `docs/generated/openapi.json` fica para o CI (ver tarefa).
