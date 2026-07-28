# Exec-plan 0013 — Cotação de filial do tomador

Status: ativo — AB#0005 (`ab-0005-cotacao-filial-tomador`). RN-052/RN-053 **aprovadas pela PO em 2026-07-28**; backend entregue e verificado (evidências abaixo), PR pendente. Move para `completed/` no mesmo PR que encerrar o backend.
Contexto obrigatório: `docs/adr/063-filial-como-pessoa-vinculada-a-matriz.md`; RN-052 em `docs/product-specs/regras-de-negocio/tomadores.md`; RN-053 e RN-050/RN-051 revisadas em `docs/product-specs/regras-de-negocio/grupo-de-cotacao.md`; RN-016 revisada em `docs/product-specs/regras-de-negocio/pessoas.md`; `docs/product-specs/open-decisions.md` (OPEN-07, OPEN-17); `ARCHITECTURE.md`; `docs/BACKEND.md`. Exec-plan irmão no front: `smartinsure-frontend/docs/exec-plans/active/0016-cotacao-filial-tomador.md`.

## Objetivo

Fazer a Filial existir como dado: cadastrada pelo Birô e vinculada à matriz (RN-052), e registrada como
**estabelecimento cotado** do Grupo de Cotação (RN-053). Hoje a filial é só um campo transitório da resposta
da busca (`PreSelectedBranchDocumentNumber`), que ninguém persiste e o front não lê.

## Migrations (`smartinsure-dbmigration`, forward-only com guards — ADRs 041–043)

- [x] `V20260727234252__adicionar-matriz-em-persons.sql` — `Persons.HeadquartersPersonId UNIQUEIDENTIFIER NULL`, FK auto-referente para `dbo.Persons (Id)`, índice filtrado `IX_Persons_HeadquartersPersonId`. Sem backfill.
- [x] `V20260728011830__adicionar-filial-em-quotation-groups.sql` — `QuotationGroups.BranchPersonId UNIQUEIDENTIFIER NULL`, FK para `dbo.Persons (Id)`, índice filtrado `IX_QuotationGroups_BranchPersonId`. Sem backfill: Rascunhos existentes seguem válidos com a matriz como estabelecimento.

> Nas duas: `GO` entre o `ALTER TABLE ADD COLUMN` e o `CREATE INDEX` que referencia a coluna nova — o SQL Server resolve nomes no compile-time do batch e falha com Error 207 sem isso — e guard do índice qualificado por `object_id = OBJECT_ID(N'dbo.<Tabela>')`, porque nome de índice é único por tabela, não por banco.

## Backend

- [x] **`Person`**: propriedade `HeadquartersPersonId` (`Guid?`) e método `LinkToHeadquarters(Person headquarters)` com as invariantes do ADR-063 — só PJ de ordem ≠ `/0001`, matriz `/0001` da mesma raiz de 8 dígitos, idempotente, recusa revínculo para outra matriz.
- [x] **`PersonMapping`**: mapear a coluna e o índice; a Filial **não** ganha Papel da Pessoa.
- [x] **`IPersonRepository`**: `GetTrackedByIdAsync` (para vincular) e `ListBranchesAsync(Guid headquartersPersonId, …)` devolvendo id, documento, nome e nome social das Filiais.
- [x] **Serviço de cadastro em cadeia** (`Application.UseCase/Services/PersonImports/`): dado um CNPJ de estabelecimento — resolve a matriz por `CnpjValidator.HeadquartersOf`, importa a matriz pelo Birô quando ausente (RN-014), importa a Filial quando ausente, vincula. Falha do Birô na matriz → nada gravado; **falha na Filial → matriz preservada**, sem vínculo, com aviso.
- [x] **`SearchPersonsUseCase`**: `ResolveHeadquartersAsync` passa a usar o serviço acima e a devolver `PreSelectedBranchId` além do documento; matriz sem Filial localizada volta sem pré-seleção, com o aviso.
- [x] **`GetPolicyHolderUseCase`**: detalhe do Tomador passa a trazer `branches[]` (RN-025).
- [x] **Novos use cases** `ListPolicyHolderBranches` e `CreatePolicyHolderBranch` (+ validators FluentValidation), expostos em `PolicyHoldersEndpoint`: `GET /{id:guid}/branches` e `POST /{id:guid}/branches`.
- [x] **`QuotationGroup`**: `BranchPersonId` (`Guid?`) em `Create` e `UpdateDraft`; `QuotationGroupMapping` com FK e índice.
- [x] **`CreateQuotationGroupUseCase` / `UpdateQuotationGroupUseCase`**: aceitam `branchId` opcional e **recusam** Filial inexistente ou vinculada a outra matriz que não o `policyHolderId` do grupo.
- [x] Testes xUnit com `[Trait("RuleId","RN-052")]` e `[Trait("RuleId","RN-053")]` cobrindo: matriz e filial ausentes; matriz existente e filial ausente; Birô falha na matriz; Birô falha na filial; filial já vinculada (sem nova consulta); Pessoa existente sem vínculo; CNPJ `/0001` recusado como filial; grupo com e sem `branchId`; filial de outra matriz recusada.
- [x] `docs/generated/openapi.json` regenerado e publicado **antes** do front consumir.

## Critérios de aceite

- CNPJ de filial informado na busca em contexto de tomador ou na ficha do Tomador resulta em matriz e Filial cadastradas e vinculadas, com a Filial identificada como pré-selecionada (RN-052, RN-016).
- Birô sem a matriz: nada é gravado. Birô sem a Filial: a matriz permanece cadastrada e utilizável, sem Filial e com aviso (RN-052).
- Filial não aparece na listagem de Tomadores nem nas buscas em contexto de tomador — que continuam devolvendo apenas matrizes (RN-016, RN-025).
- Grupo de Cotação persiste o estabelecimento cotado; ausente significa matriz; Filial de outra matriz é recusada pelo servidor (RN-053).
- Gates verdes: `dotnet build SmartInsure.slnx`, `dotnet test tests/SmartInsure.Tests` (inclui NetArchTest), `python scripts/check-harness.py`, cobertura ≥ 80%.

## Evidências

Verificado em 2026-07-28, na worktree `C:\wt\ab-0005\smartinsure-backend`, após o último commit da branch.

- **Build**: `dotnet build SmartInsure.slnx` → `0 Error(s)`, 32 warnings (todos pré-existentes, `CS8602` em testes antigos).
- **Testes**: `dotnet test tests/SmartInsure.Tests` → `Passed! - Failed: 0, Passed: 432, Skipped: 0, Total: 432`. Baseline da branch era 389; as 43 novas cobrem RN-052 e RN-053, incluindo os dois caminhos de falha do Birô, o guard de papel do Tomador e a recusa de Filial de outra matriz. NetArchTest e ConventionTests inclusos e verdes.
- **Harness**: `python scripts/check-harness.py` → `harness ok`.
- **Migrations**: as duas aplicadas ao SQL Server local via `docker compose --profile migrations up -d`; schema conferido por `INFORMATION_SCHEMA`/`sys.indexes` (coluna anulável, FK e índice filtrado presentes nas duas tabelas). Idempotência provada re-executando o corpo de cada migration contra o banco já migrado — no-op, sem objeto duplicado.
- **Contrato**: `docs/generated/openapi.json` regenerado subindo a API local e capturando `/openapi/v1.json` (+190/−3). Passa a expor `branchId` nos dois corpos de Grupo de Cotação, `preSelectedBranchId` na busca de Pessoas, `branches[]` no detalhe do Tomador e as duas rotas `policy-holders/{id}/branches`. **Arrastou junto** a correção de um drift pré-existente e alheio a esta atividade: `/api/v1/additional-coverages` estava documentado como 200 e o código já declarava 201.
- **Cobertura**: não medida localmente — o gate de 80% roda no CI. Nenhum arquivo novo ficou sem teste; os repositórios seguem sem teste próprio por decisão do ADR-057.
- **Review**: cada tarefa passou por review dedicado (spec + qualidade) com correção e re-review; ao final, um review de branch inteira nos três repositórios. Achados corrigidos, entre eles: guard de índice não qualificado por tabela nas migrations, três branches de invariante sem teste em `Person`, `preSelectedBranchDocumentNumber` sendo devolvido no caminho em que a RN-016 manda não devolver, `RegisterAsync` aceitando CNPJ inválido (gastando chamada paga ao Birô), e o self-FK de `Persons` documentado mas não modelado no EF.
