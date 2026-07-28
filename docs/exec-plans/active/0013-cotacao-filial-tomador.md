# Exec-plan 0013 — Cotação de filial do tomador

Status: ativo — AB#0005 (`ab-0005-cotacao-filial-tomador`). Aguardando **aprovação das RN-052/RN-053 pela PO** antes de qualquer código de negócio.
Contexto obrigatório: `docs/adr/063-filial-como-pessoa-vinculada-a-matriz.md`; RN-052 em `docs/product-specs/regras-de-negocio/tomadores.md`; RN-053 e RN-050/RN-051 revisadas em `docs/product-specs/regras-de-negocio/grupo-de-cotacao.md`; RN-016 revisada em `docs/product-specs/regras-de-negocio/pessoas.md`; `docs/product-specs/open-decisions.md` (OPEN-07, OPEN-17); `ARCHITECTURE.md`; `docs/BACKEND.md`. Exec-plan irmão no front: `smartinsure-frontend/docs/exec-plans/active/0016-cotacao-filial-tomador.md`.

## Objetivo

Fazer a Filial existir como dado: cadastrada pelo Birô e vinculada à matriz (RN-052), e registrada como
**estabelecimento cotado** do Grupo de Cotação (RN-053). Hoje a filial é só um campo transitório da resposta
da busca (`PreSelectedBranchDocumentNumber`), que ninguém persiste e o front não lê.

## Migrations (`smartinsure-dbmigration`, forward-only com guards — ADRs 041–043)

- [ ] `V<yyyyMMddHHmmss>__adicionar-matriz-em-persons.sql` — `Persons.HeadquartersPersonId UNIQUEIDENTIFIER NULL`, FK auto-referente para `dbo.Persons (Id)`, índice `IX_Persons_HeadquartersPersonId`. Sem backfill.
- [ ] `V<yyyyMMddHHmmss>__adicionar-filial-em-quotation-groups.sql` — `QuotationGroups.BranchPersonId UNIQUEIDENTIFIER NULL`, FK para `dbo.Persons (Id)`, índice `IX_QuotationGroups_BranchPersonId`. Sem backfill: Rascunhos existentes seguem válidos com a matriz como estabelecimento.

## Backend

- [ ] **`Person`**: propriedade `HeadquartersPersonId` (`Guid?`) e método `LinkToHeadquarters(Person headquarters)` com as invariantes do ADR-063 — só PJ de ordem ≠ `/0001`, matriz `/0001` da mesma raiz de 8 dígitos, idempotente, recusa revínculo para outra matriz.
- [ ] **`PersonMapping`**: mapear a coluna e o índice; a Filial **não** ganha Papel da Pessoa.
- [ ] **`IPersonRepository`**: `GetTrackedByIdAsync` (para vincular) e `ListBranchesAsync(Guid headquartersPersonId, …)` devolvendo id, documento, nome e nome social das Filiais.
- [ ] **Serviço de cadastro em cadeia** (`Application.UseCase/Services/PersonImports/`): dado um CNPJ de estabelecimento — resolve a matriz por `CnpjValidator.HeadquartersOf`, importa a matriz pelo Birô quando ausente (RN-014), importa a Filial quando ausente, vincula. Falha do Birô na matriz → nada gravado; **falha na Filial → matriz preservada**, sem vínculo, com aviso.
- [ ] **`SearchPersonsUseCase`**: `ResolveHeadquartersAsync` passa a usar o serviço acima e a devolver `PreSelectedBranchId` além do documento; matriz sem Filial localizada volta sem pré-seleção, com o aviso.
- [ ] **`GetPolicyHolderUseCase`**: detalhe do Tomador passa a trazer `branches[]` (RN-025).
- [ ] **Novos use cases** `ListPolicyHolderBranches` e `CreatePolicyHolderBranch` (+ validators FluentValidation), expostos em `PolicyHoldersEndpoint`: `GET /{id:guid}/branches` e `POST /{id:guid}/branches`.
- [ ] **`QuotationGroup`**: `BranchPersonId` (`Guid?`) em `Create` e `UpdateDraft`; `QuotationGroupMapping` com FK e índice.
- [ ] **`CreateQuotationGroupUseCase` / `UpdateQuotationGroupUseCase`**: aceitam `branchId` opcional e **recusam** Filial inexistente ou vinculada a outra matriz que não o `policyHolderId` do grupo.
- [ ] Testes xUnit com `[Trait("RuleId","RN-052")]` e `[Trait("RuleId","RN-053")]` cobrindo: matriz e filial ausentes; matriz existente e filial ausente; Birô falha na matriz; Birô falha na filial; filial já vinculada (sem nova consulta); Pessoa existente sem vínculo; CNPJ `/0001` recusado como filial; grupo com e sem `branchId`; filial de outra matriz recusada.
- [ ] `docs/generated/openapi.json` regenerado e publicado **antes** do front consumir.

## Critérios de aceite

- CNPJ de filial informado na busca em contexto de tomador ou na ficha do Tomador resulta em matriz e Filial cadastradas e vinculadas, com a Filial identificada como pré-selecionada (RN-052, RN-016).
- Birô sem a matriz: nada é gravado. Birô sem a Filial: a matriz permanece cadastrada e utilizável, sem Filial e com aviso (RN-052).
- Filial não aparece na listagem de Tomadores nem nas buscas em contexto de tomador — que continuam devolvendo apenas matrizes (RN-016, RN-025).
- Grupo de Cotação persiste o estabelecimento cotado; ausente significa matriz; Filial de outra matriz é recusada pelo servidor (RN-053).
- Gates verdes: `dotnet build SmartInsure.slnx`, `dotnet test tests/SmartInsure.Tests` (inclui NetArchTest), `python scripts/check-harness.py`, cobertura ≥ 80%.

## Evidências

_A preencher na execução, antes do PR — evidência antes de afirmação._

- **Build**: `dotnet build SmartInsure.slnx` → (pendente)
- **Testes**: `dotnet test tests/SmartInsure.Tests` → (pendente)
- **Harness**: `python scripts/check-harness.py` → (pendente)
- **Cobertura**: (pendente)
- **Migrations**: aplicadas via `docker compose --profile migrations` → (pendente)
- **Contrato**: `docs/generated/openapi.json` regenerado → (pendente)
