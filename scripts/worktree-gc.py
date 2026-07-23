#!/usr/bin/env python3
"""Garbage-collector de worktrees (ADR-002).

Remove as worktrees cuja branch já foi mergeada em `origin/main` — reconhecendo os
três modos de merge do GitHub: **merge commit**, **squash** e **rebase**. Assim a
worktree não fica ocupando disco depois do PR mergear.

Seguro por construção:
- `git fetch --prune` antes, para refletir o remoto.
- só remove branch mergeada — trabalho não mergeado fica.
- `git worktree remove` SEM `--force`: worktree com mudança não commitada é preservada.
- `main`/`master` e a checkout principal nunca são alvo.
- no Windows, apaga o resíduo (node_modules/.output) que o `git` deixa para trás,
  liberando o disco de fato.

Uso: python scripts/worktree-gc.py
"""
import os
import shutil
import stat
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def git(*args, check=True):
    return subprocess.run(["git", *args], cwd=ROOT, capture_output=True, text=True, check=check)


def base_remota():
    for ref in ("origin/main", "origin/master"):
        r = subprocess.run(["git", "rev-parse", "--verify", "--quiet", ref],
                           cwd=ROOT, capture_output=True, text=True)
        if r.returncode == 0:
            return ref
    return None


def _merged(base, branch):
    """True se `branch` já está em `base` (origin/main) por merge commit, squash OU rebase.

    - Merge commit / fast-forward: a branch é ancestral de `base`.
    - Squash / rebase: os commits da branch não são ancestrais, mas a árvore final dela,
      aplicada sobre o merge-base, tem patch equivalente já em `base`. Sintetizamos um
      commit dessa árvore e checamos `git cherry` (git-native, sem depender de `gh`/rede).
    """
    # Branch recém-criada, ainda na ponta de `base` (sem commits próprios): NÃO é mergeada —
    # é uma worktree de tarefa nova que ainda não commitou. `--is-ancestor` diria True (a ponta
    # está contida em `base`), o que faria o gc reapar uma worktree recém-criada — justo o
    # estado do Passo 0. No fluxo do GitHub o merge gera commit de merge em origin/main, então
    # uma branch de fato mergeada nunca fica com a ponta igual à de `base`.
    tip = subprocess.run(["git", "rev-parse", "--verify", "--quiet", branch],
                         cwd=ROOT, capture_output=True, text=True).stdout.strip()
    base_tip = subprocess.run(["git", "rev-parse", "--verify", "--quiet", base],
                              cwd=ROOT, capture_output=True, text=True).stdout.strip()
    if tip and tip == base_tip:
        return False
    anc = subprocess.run(["git", "merge-base", "--is-ancestor", branch, base],
                         cwd=ROOT, capture_output=True, text=True)
    if anc.returncode == 0:
        return True
    mb = subprocess.run(["git", "merge-base", base, branch],
                        cwd=ROOT, capture_output=True, text=True)
    if mb.returncode != 0 or not mb.stdout.strip():
        return False
    tree = subprocess.run(["git", "rev-parse", f"{branch}^{{tree}}"],
                          cwd=ROOT, capture_output=True, text=True)
    if tree.returncode != 0 or not tree.stdout.strip():
        return False
    # Identidade fixa: `commit-tree` exige committer; não depender do git config do dev.
    env = {**os.environ, "GIT_AUTHOR_NAME": "gc", "GIT_AUTHOR_EMAIL": "gc@local",
           "GIT_COMMITTER_NAME": "gc", "GIT_COMMITTER_EMAIL": "gc@local"}
    synth = subprocess.run(
        ["git", "commit-tree", tree.stdout.strip(), "-p", mb.stdout.strip(), "-m", "_gc_probe"],
        cwd=ROOT, capture_output=True, text=True, env=env)
    if synth.returncode != 0 or not synth.stdout.strip():
        return False
    cherry = subprocess.run(["git", "cherry", base, synth.stdout.strip()],
                            cwd=ROOT, capture_output=True, text=True)
    # Prefixo "-" ⇒ patch equivalente já está em `base` (mergeado por squash/rebase).
    return cherry.returncode == 0 and cherry.stdout.strip().startswith("-")


def _remover_arvore(p):
    """Apaga a pasta INTEIRA, inclusive node_modules (o que mais pesa no disco): junctions do
    pnpm, arquivos read-only e caminhos longos. No Windows usa `rmdir /s /q` (robusto pra
    node_modules e não segue junctions); complementa com shutil + chmod. Retorna True se sumiu."""
    if not p.exists():
        return True
    if os.name == "nt":
        subprocess.run(["cmd", "/c", "rmdir", "/s", "/q", str(p)], capture_output=True, text=True)
    if p.exists():
        def _on_error(func, alvo, _exc):
            try:
                os.chmod(alvo, stat.S_IWRITE)
                func(alvo)
            except OSError:
                pass
        shutil.rmtree(p, onerror=_on_error)
    return not p.exists()


def _limpar_residuo(path):
    """Remove a árvore do worktree (inclusive node_modules) e a pasta-pai <slug> se ficar vazia.
    Só é chamado após confirmar a worktree limpa. Retorna True se a pasta sumiu por completo;
    False se algo travou arquivos (ex.: dev server/editor aberto na worktree)."""
    p = Path(path)
    ok = _remover_arvore(p)
    parent = p.parent
    try:
        if parent.exists() and not any(parent.iterdir()):
            parent.rmdir()
    except OSError:
        pass
    return ok


def worktrees():
    """(path, branch) de cada worktree com branch (ignora bare/detached)."""
    out = git("worktree", "list", "--porcelain").stdout
    entradas, path, branch = [], None, None
    for linha in out.splitlines():
        if linha.startswith("worktree "):
            path, branch = linha[len("worktree "):], None
        elif linha.startswith("branch "):
            branch = linha[len("branch "):].replace("refs/heads/", "")
        elif linha == "" and path:
            entradas.append((path, branch))
            path, branch = None, None
    if path:
        entradas.append((path, branch))
    return entradas


def main():
    try:
        git("rev-parse", "--git-dir")
    except (OSError, subprocess.CalledProcessError):
        print("git indisponível — nada a fazer")
        return 0

    print("worktree-gc — git fetch --prune…")
    git("fetch", "--prune", check=False)

    base = base_remota()
    if base is None:
        print("  sem origin/main — nada a avaliar")
        return 0

    # A branch da worktree atual está atrás de origin/main? Aviso para sincronizar antes de
    # commitar/abrir PR (ADR-002 — nenhuma branch abre PR atrás da main). Reaproveita o
    # `git fetch --prune` feito acima; não altera nada, só avisa.
    atual = subprocess.run(["git", "rev-parse", "--abbrev-ref", "HEAD"],
                           cwd=ROOT, capture_output=True, text=True)
    branch_atual = atual.stdout.strip() if atual.returncode == 0 else ""
    if branch_atual and branch_atual not in ("main", "master", "HEAD"):
        r = subprocess.run(["git", "rev-list", "--count", f"HEAD..{base}"],
                           cwd=ROOT, capture_output=True, text=True)
        atras = r.stdout.strip() if r.returncode == 0 else ""
        if atras.isdigit() and int(atras) > 0:
            print(f"  [sync] a branch atual '{branch_atual}' está {atras} commit(s) atrás de "
                  f"{base} — rode: git pull origin main (sincronize antes de commitar/abrir PR).")

    removidas, preservadas, parciais = [], [], []
    for path, branch in worktrees():
        if not branch or branch in ("main", "master"):
            continue
        # NUNCA a worktree atual (de onde o gc roda): mesmo com a branch mergeada, remover a
        # worktree onde você está apagaria seu diretório de trabalho. O `git worktree remove`
        # recusaria a atual, mas a limpeza de resíduo (rmdir) rodaria mesmo assim — então pulamos.
        if Path(path).resolve() == ROOT:
            continue
        if not _merged(base, f"refs/heads/{branch}"):
            continue
        # Só limpa worktree SEM trabalho não commitado. node_modules/.output são ignorados
        # (git status não os vê); status sujo = trabalho real → preservar (nunca usar --force).
        st = subprocess.run(["git", "-C", path, "status", "--porcelain"],
                            cwd=ROOT, capture_output=True, text=True)
        if st.returncode != 0 or st.stdout.strip():
            preservadas.append((path, branch,
                                "trabalho não commitado" if st.returncode == 0 else st.stderr.strip()))
            continue
        # Worktree limpa: desregistra e apaga a árvore INTEIRA (inclusive node_modules). O
        # `git worktree remove` no Windows desregistra mas costuma deixar node_modules — por isso
        # a remoção real da pasta é do `_limpar_residuo`; o `prune` no fim fecha o registro.
        subprocess.run(["git", "worktree", "remove", path], cwd=ROOT, capture_output=True, text=True)
        if _limpar_residuo(path):
            removidas.append((path, branch, ""))
        else:
            parciais.append((path, branch,
                             "node_modules/arquivos travados — feche o dev server/editor apontando "
                             "para essa worktree e rode o gc de novo"))

    git("worktree", "prune", check=False)

    for path, branch, _ in removidas:
        print(f"  [removida]   {branch} -> {path}")
    for path, branch, err in parciais:
        print(f"  [parcial]    {branch} -> {path} ({err})")
    for path, branch, err in preservadas:
        print(f"  [preservada] {branch} -> {path} (não removida: {err or 'em uso/pendências'})")
    if not removidas and not preservadas and not parciais:
        print("  nada a limpar — nenhuma worktree mergeada.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
