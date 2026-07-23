#!/usr/bin/env python3
"""Doctor do workspace SmartInsure (ADR-001 / ADR-002).

Valida o ambiente local: versão do Python e o layout de workspace (os dois
repos como irmãos lado a lado, para os ponteiros `../smartinsure-<repo>/`
resolverem). Advisório — ajuda o dev a montar o ambiente; não é gate de CI.
Sai 0 sempre; imprime [ok]/[!] por checagem.

Uso: python scripts/doctor.py
"""
import os
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
NOME = ROOT.name
PAR = "smartinsure-frontend" if NOME == "smartinsure-backend" else "smartinsure-backend"
# o front depende dos ponteiros ../smartinsure-backend; para ele o par é essencial.
PAR_ESSENCIAL = NOME == "smartinsure-frontend"


def _merged(base, branch):
    """True se `branch` já está em `base` por merge commit, squash OU rebase (mesma regra do
    worktree-gc). Squash/rebase: sintetiza um commit da árvore da branch sobre o merge-base e
    checa `git cherry` — git-native, sem depender de `gh`/rede."""
    if subprocess.run(["git", "merge-base", "--is-ancestor", branch, base],
                      cwd=ROOT, capture_output=True, text=True).returncode == 0:
        return True
    mb = subprocess.run(["git", "merge-base", base, branch], cwd=ROOT, capture_output=True, text=True)
    if mb.returncode != 0 or not mb.stdout.strip():
        return False
    tree = subprocess.run(["git", "rev-parse", f"{branch}^{{tree}}"], cwd=ROOT, capture_output=True, text=True)
    if tree.returncode != 0 or not tree.stdout.strip():
        return False
    env = {**os.environ, "GIT_AUTHOR_NAME": "gc", "GIT_AUTHOR_EMAIL": "gc@local",
           "GIT_COMMITTER_NAME": "gc", "GIT_COMMITTER_EMAIL": "gc@local"}
    synth = subprocess.run(
        ["git", "commit-tree", tree.stdout.strip(), "-p", mb.stdout.strip(), "-m", "_gc_probe"],
        cwd=ROOT, capture_output=True, text=True, env=env)
    if synth.returncode != 0 or not synth.stdout.strip():
        return False
    cherry = subprocess.run(["git", "cherry", base, synth.stdout.strip()], cwd=ROOT, capture_output=True, text=True)
    return cherry.returncode == 0 and cherry.stdout.strip().startswith("-")

oks, avisos = [], []

v = sys.version_info
(oks if v >= (3, 9) else avisos).append(f"Python {v.major}.{v.minor}" + ("" if v >= (3, 9) else " — o lint pede 3.9+"))

irmao = ROOT.parent / PAR
if irmao.exists():
    oks.append(f"workspace: {PAR} está lado a lado")
else:
    msg = (f"{PAR} não encontrado em {irmao} — clone/worktree os dois repos como "
           f"irmãos na mesma pasta (ex.: C:\\src\\smart\\ ou C:\\wt\\ab-NNNNN\\)")
    avisos.append(("IMPORTANTE: " + msg + " — sem isso os ponteiros de produto "
                   "(glossário, RNs) não resolvem.") if PAR_ESSENCIAL else msg)

raiz = str(ROOT)
if len(raiz) > 90 or " " in raiz:
    avisos.append(f"raiz do repo longa ou com espaço ({raiz}) — o ADR-002 pede raiz curta e sem espaço "
                  f"(ex.: C:\\src\\smart\\) por causa do MAX_PATH no Windows")

lint = ROOT / "scripts" / "check-harness.py"
(oks if lint.exists() else avisos).append("check-harness.py presente" if lint.exists() else "check-harness.py ausente")

# Worktree da tarefa (ADR-002): o trabalho começa numa worktree do repo, nunca na checkout
# principal — permite atividades paralelas isoladas em terminais separados. Advisório.
try:
    gd = subprocess.run(["git", "rev-parse", "--git-dir"], cwd=ROOT,
                        capture_output=True, text=True, check=True).stdout.strip()
    gcd = subprocess.run(["git", "rev-parse", "--git-common-dir"], cwd=ROOT,
                         capture_output=True, text=True, check=True).stdout.strip()
    if Path(gd).resolve() != Path(gcd).resolve():
        oks.append("worktree: trabalhando numa worktree da tarefa")
    else:
        avisos.append("checkout principal (não é worktree) — toda tarefa começa numa worktree "
                      "do(s) repo(s) da triagem (ADR-002): C:\\wt\\<slug>\\<repo>, irmãs sob o "
                      "mesmo <slug> se cross-repo. Provisório: <slug> até existir o AB#.")
except (OSError, subprocess.CalledProcessError):
    pass  # git indisponível — checagem pulada

# Worktrees já mergeadas ocupam disco (ADR-002): heads-up para rodar o worktree-gc. Só lê o
# estado local (sem fetch) — o gc faz o fetch antes de remover.
try:
    _base = None
    for _ref in ("origin/main", "origin/master"):
        if subprocess.run(["git", "rev-parse", "--verify", "--quiet", _ref], cwd=ROOT,
                          capture_output=True, text=True).returncode == 0:
            _base = _ref
            break
    if _base:
        _wl = subprocess.run(["git", "worktree", "list", "--porcelain"], cwd=ROOT,
                             capture_output=True, text=True, check=True).stdout
        _orfas = 0
        for _linha in _wl.splitlines():
            if _linha.startswith("branch "):
                _br = _linha[len("branch "):].replace("refs/heads/", "")
                if _br not in ("main", "master") and _merged(_base, f"refs/heads/{_br}"):
                    _orfas += 1
        if _orfas:
            avisos.append(f"{_orfas} worktree(s) com branch já mergeada ocupando disco — "
                          f"rode: python scripts/worktree-gc.py (ADR-002).")
except (OSError, subprocess.CalledProcessError):
    pass

print(f"doctor — {NOME}")
for o in oks:
    print(f"  [ok] {o}")
for a in avisos:
    print(f"  [!]  {a}")
print("ambiente ok" if not avisos else "ambiente com avisos (veja acima)")
