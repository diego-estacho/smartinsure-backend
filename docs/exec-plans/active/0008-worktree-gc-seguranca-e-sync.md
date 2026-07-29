# Exec-plan 0008 — worktree-gc: não apagar a worktree atual + aviso de branch dessincronizada

Status: em andamento
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `docs/adr/002-convencoes-de-git.md` (ADR-002 — worktree por tarefa, limpeza pós-merge e sync antes do push), `scripts/worktree-gc.py`, `scripts/doctor.py`. Cópias irmãs no `smartinsure-frontend` (mesma correção; PR linkado pelo slug `gc-safe-sync`).

## Objetivo

1. **Corrigir bug destrutivo** no `worktree-gc.py` (entrou na main via #16/#25): rodado no **Passo 0** de dentro de uma worktree recém-criada, o gc **apagava a própria worktree atual**. Causa: uma branch nova (sem commits) é idêntica a `origin/main` → `git merge-base --is-ancestor` a considera "mergeada" → o gc a reapa. Como o `git status` está limpo, passava na pré-checagem e o `rmdir /s /q` deletava tudo. Dispara exatamente no cenário documentado ("rodar o gc no início da tarefa").
2. **Formalizar o sync da branch** (pedido do dono): manter as branches sempre sincronizadas com a main do GitHub — `git pull origin main` na branch antes do push/PR — com aviso automático no gc.

## Triagem (Passo 0)

Harness/ferramenta — não toca negócio nem contrato (não dispara o gate de exec-plan de `src/`). Cross-repo por **espelhamento**: scripts e ADR canônicos no backend; o `smartinsure-frontend` carrega cópias de `worktree-gc.py`/`doctor.py` com a mesma correção. Branches `gc-safe-sync` nos dois repos, PRs linkados pelo slug. **Descoberto por dogfooding**: rodar o Passo 0 desta própria atividade destruiu as worktrees recém-criadas (nenhum trabalho perdido — estavam limpas), o que expôs o bug.

## Escopo

**Dentro:**
- `scripts/worktree-gc.py` (back + front): (a) **nunca remover a worktree atual** — `Path(path).resolve() == ROOT` → pula, mesmo com a branch mergeada; (b) **guarda de branch-nova** no `_merged` — se a ponta da branch é igual à de `origin/main` (sem commits próprios), não é "mergeada" → não reapar; (c) **aviso `[sync]`** — se a branch atual está atrás de `origin/main`, imprime `rode git pull origin main` (reaproveita o `git fetch --prune` que já roda).
- `scripts/doctor.py` (back + front): mesma guarda de branch-nova no `_merged` e não contar a worktree atual como órfã (coerência com o gc).
- `docs/adr/002-convencoes-de-git.md`: nota de segurança (nunca a worktree atual/branch nova) + convenção "sincronizar a branch antes do push".
- `AGENTS.md` (back + front): "antes do push/PR, `git pull origin main` na branch" na convenção de worktree (dentro do limite de 100 linhas).

**Fora (e por quê):**
- Git hook de pre-push: kit per-dev é vedado (ADR-003); o mecanismo compartilhado é a doc + o aviso advisório do gc (que já faz fetch no Passo 0).

## Tarefas

- [x] Guarda "nunca a worktree atual" no `worktree-gc.py` (back + front).
- [x] Guarda de branch-nova (`tip == base_tip → False`) no `_merged` do `worktree-gc.py` e do `doctor.py` (back + front).
- [x] Aviso `[sync]` de branch atrás de `origin/main` no `worktree-gc.py` (back + front).
- [x] `doctor.py` não conta a worktree atual como órfã (back + front).
- [x] ADR-002 (segurança + sync) e `AGENTS.md` (sync antes do push) atualizados.
- [x] `check-harness` verde nos dois repos + teste sintético cobrindo os dois cenários destrutivos.

## Critérios de aceite

- `python scripts/check-harness.py` verde nos dois repos; `AGENTS.md` ≤ 100 linhas.
- Rodar o gc de dentro de uma worktree **recém-criada** NÃO a remove (regressão do incidente).
- Rodar o gc de dentro de uma worktree cuja branch **já foi mergeada** NÃO a remove (é a worktree atual).
- Irmã de fato mergeada continua sendo removida; irmã recém-criada é preservada.
- Branch atrás de `origin/main` gera o aviso `[sync]`.

## Evidências

**Gates:**
- `python -m py_compile` ok em `worktree-gc.py` e `doctor.py` (back + front).
- `python scripts/check-harness.py` → `harness ok` nos dois repos; `AGENTS.md` dentro do limite.

**Teste de segurança (repo git descartável, dois cenários) — TODOS PASSARAM:**
- **Cenário A — gc rodado de uma worktree FRESCA (o incidente):** a worktree atual (fresca) é **preservada**; as irmãs de fato mergeadas (`feat`, `cur`) são **removidas**.
- **Cenário B — gc rodado de uma worktree cuja branch JÁ foi mergeada:** a worktree atual (mergeada) é **preservada** (guarda "nunca a atual"); irmã mergeada **removida**; irmã fresca **preservada**; e o aviso `[sync] a branch atual 'cur' está 1 commit(s) atrás de origin/main` disparou corretamente.

**Dogfood real (Passo 0 desta atividade):** as worktrees `gc-safe-sync` (branch nova) rodaram o gc corrigido e foram preservadas; a `harness-gc-cleanup` (já mergeada nos #16/#25) foi removida e o disco liberado.
