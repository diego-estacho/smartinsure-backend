#!/usr/bin/env python3
"""Doctor do workspace SmartInsure (ADR-001 / ADR-002).

Valida o ambiente local: versão do Python e o layout de workspace (os dois
repos como irmãos lado a lado, para os ponteiros `../smartinsure-<repo>/`
resolverem). Advisório — ajuda o dev a montar o ambiente; não é gate de CI.
Sai 0 sempre; imprime [ok]/[!] por checagem.

Uso: python scripts/doctor.py
"""
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
NOME = ROOT.name
PAR = "smartinsure-frontend" if NOME == "smartinsure-backend" else "smartinsure-backend"
# o front depende dos ponteiros ../smartinsure-backend; para ele o par é essencial.
PAR_ESSENCIAL = NOME == "smartinsure-frontend"

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

print(f"doctor — {NOME}")
for o in oks:
    print(f"  [ok] {o}")
for a in avisos:
    print(f"  [!]  {a}")
print("ambiente ok" if not avisos else "ambiente com avisos (veja acima)")
