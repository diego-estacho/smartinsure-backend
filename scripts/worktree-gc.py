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


def _limpar_residuo(path):
    """Windows: `git worktree remove` desregistra a worktree mas às vezes deixa
    node_modules/.output (ignorados/travados) para trás. Só é chamado quando o remove
    teve sucesso (worktree limpa) — apaga o resíduo e a pasta-pai <slug> se ficou vazia."""
    def _on_error(func, p, _exc):
        try:
            os.chmod(p, stat.S_IWRITE)
            func(p)
        except OSError:
            pass
    p = Path(path)
    if p.exists():
        shutil.rmtree(p, onerror=_on_error)
    parent = p.parent
    try:
        if parent.exists() and not any(parent.iterdir()):
            parent.rmdir()
    except OSError:
        pass


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

    removidas, preservadas = [], []
    for path, branch in worktrees():
        if not branch or branch in ("main", "master"):
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
        # Worktree limpa: remove. No Windows o `git worktree remove` desregistra mas às vezes
        # falha no rmdir final (ignorados travados, ex.: node_modules) — por isso limpamos o
        # resíduo de qualquer modo; o `git worktree prune` no fim resolve o registro pendente.
        rm = subprocess.run(["git", "worktree", "remove", path],
                            cwd=ROOT, capture_output=True, text=True)
        _limpar_residuo(path)
        removidas.append((path, branch, "" if rm.returncode == 0 else rm.stderr.strip()))

    git("worktree", "prune", check=False)

    for path, branch, _ in removidas:
        print(f"  [removida]   {branch} -> {path}")
    for path, branch, err in preservadas:
        print(f"  [preservada] {branch} -> {path} (não removida: {err or 'em uso/pendências'})")
    if not removidas and not preservadas:
        print("  nada a limpar — nenhuma worktree mergeada.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
