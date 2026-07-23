# Exec-plan 0007 — worktree-gc: detecção squash/rebase-aware + reclamar disco no Windows

Status: em andamento
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `docs/adr/002-convencoes-de-git.md` (ADR-002 — worktree por tarefa e limpeza pós-merge), `scripts/worktree-gc.py`, `scripts/doctor.py`. Cópias irmãs no `smartinsure-frontend` (mesma correção; PR linkado pelo slug `harness-gc-cleanup`).

## Objetivo

Fazer a limpeza de worktrees pós-merge **realmente acontecer** e **liberar disco de fato**:

1. **Detectar branch mergeada por squash e rebase**, não só por merge commit. Hoje o critério é `git merge-base --is-ancestor <branch> origin/main`, que só reconhece merge commit / fast-forward. PR mergeado por **Squash** ou **Rebase** (ambos habilitados no repo) fica eternamente "não mergeado" → a worktree nunca é removida e ocupa disco.
2. **Reclamar o disco no Windows.** O `git worktree remove` desregistra a worktree mas deixa `node_modules`/`.output` órfãos (arquivos ignorados/travados) — foi preciso apagar ~450 MB à mão. Passar a remover o resíduo e a pasta-pai `<slug>` se ficar vazia.

## Triagem (Passo 0)

Harness/ferramenta — não toca comportamento de negócio nem contrato (não dispara o gate de exec-plan de `src/`, ADR-059). Cross-repo por **espelhamento**: o script canônico e o ADR vivem no backend; o `smartinsure-frontend` carrega cópias de `worktree-gc.py`/`doctor.py` que precisam da mesma correção. Branches `harness-gc-cleanup` nos dois repos, PRs linkados pelo slug (sem `AB#`).

## Escopo

**Dentro:**
- `scripts/worktree-gc.py`: helper `_merged(base, branch)` — mantém o caminho rápido `is-ancestor` (merge commit / ff) e adiciona fallback **git-native** para squash **e** rebase: sintetiza um commit da árvore final da branch sobre o `merge-base` e checa `git cherry origin/main <synth>` (patch equivalente já upstream ⇒ mergeada). Sem dependência de `gh`/rede.
- `scripts/worktree-gc.py`: após `git worktree remove` bem-sucedido, apagar o diretório residual (rmtree resiliente a arquivo read-only do Windows) e a pasta-pai `<slug>` se vazia.
- `scripts/doctor.py`: mesma detecção no aviso de worktree órfã (coerência com o gc).
- `docs/adr/002-convencoes-de-git.md`: ampliar o critério de "mergeada" (squash/rebase, não só "contida em origin/main") e explicitar a reclamação total de disco.
- `AGENTS.md`: formalizar o `worktree-gc` como parte do **Passo 0** (início de tarefa), não só "após o merge" — respeitando o limite de 100 linhas.
- Espelho no `smartinsure-frontend`: mesmos 2 scripts + AGENTS.md (fino).

**Fora (e por quê):**
- Automação por daemon/git-hook: o merge do PR é remoto (GitHub), sem evento local para reagir; e kit per-dev é vedado (ADR-003). O gatilho segue sendo o `doctor.py` (advisory) + rodar no Passo 0.
- Detecção via `gh`: evita acoplar a limpeza a rede/credencial; a via git-native cobre os três modos de merge.

## Tarefas

- [x] `_merged()` squash/rebase-aware no `worktree-gc.py` (backend + frontend).
- [x] rmtree do resíduo + pasta-pai vazia no `worktree-gc.py` (backend + frontend).
- [x] mesma detecção no `doctor.py` (backend + frontend).
- [x] `docs/adr/002-convencoes-de-git.md` atualizado (critério de mergeada + disco).
- [x] `AGENTS.md`: `worktree-gc` no Passo 0 (backend + frontend), dentro do limite de linhas.
- [x] Gates (`check-harness`) verdes nos dois repos + dogfood (limpar a worktree `app-shell-nav` já mergeada) + teste sintético de squash.

## Critérios de aceite

- `python scripts/check-harness.py` verde nos dois repos; `AGENTS.md` ≤ 100 linhas.
- Branch mergeada por **merge commit**, **squash** e **rebase** é reconhecida como mergeada; branch não mergeada continua preservada.
- Após remover, o diretório da worktree e a pasta-pai `<slug>` (se vazia) somem do disco.
- Trabalho não-commitado continua preservado (remoção sem `--force`).

## Evidências

**Gates:**
- `python scripts/check-harness.py` → `harness ok` nos dois repos; `AGENTS.md` em 66 (backend) / 70 (frontend) linhas (≤ 100).
- `python -m py_compile` ok em `worktree-gc.py` e `doctor.py` (ambos os repos).

**Detecção (teste sintético em repo git temporário):**
- Mergeada por **squash** → reconhecida como mergeada (via `commit-tree` da árvore + `git cherry`).
- Mergeada por **rebase** (patches replayed) → reconhecida como mergeada.
- **Não mergeada** → preservada.

**Fluxo de remoção + reclamação de disco (Windows):**
- Bug pego no 1º dogfood: `git worktree remove` retornou "Directory not empty" (desregistrou a worktree, mas deixou `node_modules` travado); a versão inicial só limpava o resíduo quando `returncode == 0` → não reclamava o disco. Corrigido: **pré-checagem de limpeza** via `git status --porcelain` (trabalho real preservado; arquivos ignorados não contam) + `_limpar_residuo` **sempre** após o remove, com `git worktree prune` ao fim.
- Hardening da deleção (feedback do dono — node_modules é o que mais pesa): a pasta **inteira** tem de sumir, inclusive `node_modules`. `shutil.rmtree` é frágil no Windows com junctions do pnpm / read-only / caminhos longos, e o `onerror` silencioso podia deixar resíduo reportando "removida". Passou a usar **`rmdir /s /q`** (nativo, robusto e não segue junctions) + fallback `shutil`+chmod, e **verifica** que a pasta sumiu — se algo travar (dev server/editor aberto), classifica **[parcial]** com orientação, nunca "removida" falso.
- **Teste com `node_modules` real do pnpm**: worktree mergeada com **415 MB e 2944 junctions** → gc a classifica **[removida]** e a árvore inteira (node_modules + pasta-pai `<slug>`) some do disco por completo.
- Limpeza real: worktree `app-shell-nav` (PR #24 mergeado) reclamada — **450 MB**.
- Worktree **não-mergeada** (`harness-gc-cleanup`, esta atividade, com commit à frente) preservada em todos os runs.
