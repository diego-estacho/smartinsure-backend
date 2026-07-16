#!/usr/bin/env python3
"""Garbage-collector de worktrees (ADR-002).

Remove as worktrees cuja branch já foi mergeada — critério: a branch está
totalmente contida em `origin/main` (todos os commits já estão lá), que é o
estado após o PR mergear. Assim a worktree não fica ocupando disco.

Seguro por construção:
- `git fetch --prune` antes, para refletir o remoto.
- só remove branch mergeada (is-ancestor de origin/main) — trabalho não mergeado fica.
- `git worktree remove` SEM `--force`: worktree com mudança não commitada é preservada.
- `main`/`master` e a checkout principal nunca são alvo.

Uso: python scripts/worktree-gc.py
"""
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
        mergeada = subprocess.run(
            ["git", "merge-base", "--is-ancestor", f"refs/heads/{branch}", base],
            cwd=ROOT, capture_output=True, text=True).returncode == 0
        if not mergeada:
            continue
        rm = subprocess.run(["git", "worktree", "remove", path],
                            cwd=ROOT, capture_output=True, text=True)
        (removidas if rm.returncode == 0 else preservadas).append(
            (path, branch, rm.stderr.strip()))

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
