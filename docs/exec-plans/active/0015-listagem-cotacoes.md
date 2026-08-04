# Exec-plan 0015 — Listagem de Cotações (o "livro" da Corretora, read-only — backend)

Status: **em andamento** (2026-07-30). Slug `listagem-cotacoes`, AB# pendente. Fatia 1 (read-only) da Listagem de Cotações; o **cancelamento** é Fatia 2 (demanda própria). Precede o rename `Automatic → ReadyForEmission` (já mergeado, PRs #28/#35/#17).
Contexto obrigatório: `AGENTS.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, `docs/QUALITY_SCORE.md`, **RN-077/RN-078** (`regras-de-negocio/cotacao.md`), RN-058 (classificação), RN-064 (Escopo ativo), glossário (Cotação, Grupo de Cotação, situação apresentada da Cotação), [ADR-064](../../adr/064-classificacao-resultado-cotacao.md), ADR-012 (`PagedResponse`), ADR-038 (DTO de projeção), [ADR-063](../../adr/063-exportacao-listagens-excel.md), `.grill/listagem-de-cotacoes.md`.

## Objetivo

O passo-4 persiste `Quotation` por `QuotationGroup`, mas só há leitura **por grupo** (`GET /quotation-groups/{id}` e o leque `ListQuotations`). Não existe endpoint que liste as Cotações da Corretora de forma **achatada** — logo não há "livro de cotações" para o corretor achar/triar o que já cotou. Esta fatia entrega a **leitura paginada e filtrável** das Cotações da Corretora do **Escopo ativo**, read-only, e publica o contrato para o front consumir.

## Escopo e não-escopo

- **No escopo:**
  - `GET /quotations` — listagem paginada (`PagedResponse`, ADR-012), **uma linha por `Quotation`** achatando todos os Grupos, da Corretora do Escopo ativo (RN-077). Ordenada por data de obtenção desc. Cada item: número (`ProposalNumber`), Tomador, Segurado, Seguradora (+logo), Modalidade, importância segurada, prêmio e comissão (quando aplicáveis — RN-058), resultado (**nome estável** → situação apresentada RN-078), vigência (início/fim), criada em.
  - **Inclusão (RN-077):** só `ProcessingStatus = Obtained` com resultado de **origem provedor**; exclui `Requested` (em voo), `Failed` (técnica) e `Unavailable` de origem **`Local`** ("não incluída na solicitação").
  - **Visibilidade (RN-064):** Corretora do Escopo ativo (`ICurrentUserAccessor.ActiveBrokerageId`) via `QuotationGroup.BrokerageId`. Sem Escopo de Corretora → recusa (fail-closed, SECURITY.md).
  - **Filtros (E lógico):** busca livre (número/tomador/segurado/seguradora/modalidade), situação (resultado), Seguradora, Modalidade, faixa de prêmio, faixa de IS, período de criação, período de início de vigência. **Contagem por situação apresentada** (RN-078) respeitando os demais filtros.
  - **Opções de filtro:** distintos de Seguradora e Modalidade **presentes no livro** da Corretora (na resposta ou endpoint auxiliar).
- **Fora do escopo (com motivo):**
  - **Tela (front):** exec-plan próprio no repo do front; esta fatia só publica o contrato.
  - **Cancelamento** (Fatia 2 — estado do eixo-2 + cancel no PLUG), **modal de detalhes** (vai mudar), **"Continuar"/re-entrada**, **emissão/followup**.
  - **Recorte por usuário individual** (meu × time): a visibilidade é por Corretora ativa; o filtro fino por Perfil depende de RN-072/[OPEN-03], adiado.
  - **Exportação Excel** (ADR-063): reaproveitável, mas fora desta fatia.

## Tarefas

- [ ] DTO de projeção `QuotationBookItemDto` em `Core/Abstractions/Repositories/Dtos` (join `Quotation` × `QuotationGroup` × `Insurer` × `Modality` × `Person` tomador/segurado, ADR-038).
- [ ] `IQuotationRepository`: `ListBookAsync` (paginado/filtrado/escopado por `BrokerageId`, `AsNoTracking`, projeção), `CountByResultAsync` (contagem por situação com os mesmos filtros), `ListBookInsurersAndModalitiesAsync` (distintos para as opções de filtro).
- [ ] `ListQuotationBookUseCase` + Request (filtros/paginação saneados) + Response (`PagedResponse` + contagens + opções). Inclusão/exclusão e situação apresentada derivadas no servidor por **nome estável** (ADR-031).
- [ ] `QuotationsEndpoint`: `GET /` autenticado, exige Escopo de Corretora (sem policy de Admin — é livro da própria Corretora).
- [ ] Testes de use case/repo `[Trait("RuleId","RN-077"|"RN-078")]`: inclusão (só Obtained-provider), exclusão (Requested/Failed/Local), escopo por Corretora ativa, filtros combinados, contagem por situação, situação apresentada por nome estável, paginação saneada.
- [ ] `dotnet build`, `dotnet test`, `check-harness.py`; **regenerar `docs/generated/openapi.json`** para o front.

## Critérios de aceite

- `GET /quotations` devolve `PagedResponse` com página/tamanho saneados, contendo **apenas** Cotações `Obtained` com resultado do provedor da Corretora do Escopo ativo; nunca Cotações em voo, falhas técnicas nem indisponibilidades locais.
- Sem filtros, traz Cotações em qualquer situação; cada filtro restringe e todos valem em conjunto; a resposta traz o total e a **contagem por situação apresentada** considerando os demais filtros.
- Cada item traz número (ou vazio quando a Seguradora não informou), Tomador, Segurado, Seguradora, Modalidade, IS, prêmio/comissão quando aplicáveis, resultado por nome estável e vigência.
- As opções de Seguradora/Modalidade contemplam só os valores presentes no livro da Corretora.
- Requisição sem Escopo de Corretora é recusada (fail-closed). Situação sai por **nome estável** (ADR-031), nunca por índice.
- Sem migration nova; nenhuma escrita alterada.

## Evidências

- (preenchida ao concluir: `dotnet build`/`test` verdes com contagem, `check-harness.py`, `openapi.json` regenerado com `/api/v1/quotations`.)
