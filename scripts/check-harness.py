#!/usr/bin/env python3
"""Lint mecânico do harness SmartInsure (backend).

Valida que a base de conhecimento está íntegra:
  1. Links markdown relativos não quebrados.
  2. Referências a arquivos .md em prosa (fora de código/links) resolvem.
  3. Referências a IDs (ADR-###, OPEN-##, RN-NNN) resolvem.
  4. Todo index.md lista os .md irmãos da pasta.
  5. Cada OPEN-NN de open-decisions tem 'Dono:' e 'Bloqueia:'.
  6. Cada RN (bloco '## RN-NNN') tem as seções obrigatórias e ID único no catálogo.
  7. AGENTS.md continua um mapa (máximo 150 linhas).
  8. Exec-plan ativo tem seção 'Evidências'; exec-plan concluído a tem preenchida.
  9. Cada ADR em docs/adr/ tem título '# ADR-NNN' e seção '## Status'.
 10. Nenhuma pasta de framework de desenvolvimento está versionada (ADR-004).

.git/, .claude/ e node_modules/ ficam fora do lint — não são documentação do projeto.

Uso: python scripts/check-harness.py   (exit 1 se algo falhar)
"""
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
IRMAO = ROOT.parent / "smartinsure-frontend"
IGNORAR = (".git", ".claude", "node_modules")
# arquivos gerados por script (Fase B) — citados antes de existir:
MD_FUTUROS = {"db-schema.md", "rastreabilidade-rn.md"}
SECOES_RN = ("Descrição", "Pré-condições", "Critério de aceitação", "Casos limite")

LINK_RE = re.compile(r"\[[^\]]*\]\(([^)\s#]+)(?:#[^)]*)?\)")
FENCE_RE = re.compile(r"```.*?```", re.S)
INLINE_RE = re.compile(r"`[^`]*`")
MD_NAME_RE = re.compile(r"\b[\w][\w.\-]*\.md\b")
ADR_RE = re.compile(r"\bADR-(\d{3})\b")
OPEN_RE = re.compile(r"\bOPEN-(\d{2})\b")
RN_RE = re.compile(r"\bRN-\d{3}\b")
RN_BLOCO_RE = re.compile(r"^##\s+(RN-\d{3})\b(.*?)(?=^##\s|\Z)", re.M | re.S)

errors, warnings = [], []


def rel(p):
    return str(p.relative_to(ROOT)).replace("\\", "/")


def sem_codigo(texto):
    """Remove blocos de código, código inline e links — sobra a prosa."""
    texto = FENCE_RE.sub(" ", texto)
    texto = INLINE_RE.sub(" ", texto)
    return LINK_RE.sub(" ", texto)


md_files = [f for f in ROOT.rglob("*.md")
            if not any(p in f.parts for p in IGNORAR)]

irmao_presente = IRMAO.exists()
basenames = {f.name for f in md_files} | MD_FUTUROS
if irmao_presente:
    basenames |= {f.name for f in IRMAO.rglob("*.md")
                  if not any(p in f.parts for p in IGNORAR)}

# IDs definidos
adr_dir = ROOT / "docs" / "adr"
adr_files = sorted(adr_dir.glob("[0-9][0-9][0-9]-*.md")) if adr_dir.exists() else []
if not adr_files:
    errors.append("docs/adr/ não existe ou não tem ADR no formato NNN-titulo.md")
adrs_def = {f.name[:3] for f in adr_files}

od_path = ROOT / "docs" / "product-specs" / "open-decisions.md"
opens_def = set(re.findall(r"^##\s*OPEN-(\d{2})", od_path.read_text(encoding="utf-8"), re.M)) \
    if od_path.exists() else set()

rn_dir = ROOT / "docs" / "product-specs" / "regras-de-negocio"
rn_files = [f for f in rn_dir.glob("*.md")
            if f.name != "README.md" and not f.name.startswith("_")] if rn_dir.exists() else []
rn_def = set()
for f in rn_files:
    rn_def |= set(RN_RE.findall(f.read_text(encoding="utf-8")))

# --- por arquivo ---
for f in md_files:
    texto = f.read_text(encoding="utf-8")

    # 1. links markdown relativos
    for m in LINK_RE.finditer(texto):
        alvo = m.group(1)
        if alvo.startswith(("http://", "https://", "mailto:")):
            continue
        if not (f.parent / alvo).exists():
            errors.append(f"{rel(f)}: link quebrado -> {alvo}")

    if f.name.startswith("_"):
        continue  # templates: exemplos ilustrativos não são referências reais

    prosa = sem_codigo(texto)

    # 2. .md citado em prosa
    for nome in MD_NAME_RE.findall(prosa):
        if nome in basenames:
            continue
        if irmao_presente:
            errors.append(f"{rel(f)}: referência a .md inexistente em prosa -> {nome}")
        else:
            warnings.append(f"{rel(f)}: .md citado em prosa não verificado (irmão ausente) -> {nome}")

    # 3. IDs
    for num in ADR_RE.findall(texto):
        if num not in adrs_def:
            errors.append(f"{rel(f)}: referência a ADR inexistente -> ADR-{num}")
    for num in OPEN_RE.findall(texto):
        if num not in opens_def:
            errors.append(f"{rel(f)}: referência a OPEN inexistente -> OPEN-{num}")
    for rid in RN_RE.findall(texto):
        if rid not in rn_def:
            errors.append(f"{rel(f)}: referência a RN não catalogada -> {rid}")

# 4. index.md lista os irmãos da pasta
for idx in ROOT.rglob("index.md"):
    if any(p in idx.parts for p in IGNORAR):
        continue
    txt = idx.read_text(encoding="utf-8")
    for sib in idx.parent.glob("*.md"):
        if sib.name == "index.md" or sib.name.startswith("_"):
            continue
        if sib.name not in txt:
            errors.append(f"{rel(idx)}: índice desatualizado — não lista {sib.name}")

# 5. open-decisions: Dono e Bloqueia
if od_path.exists():
    for chunk in re.split(r"^## ", od_path.read_text(encoding="utf-8"), flags=re.M)[1:]:
        title = chunk.splitlines()[0].strip()
        if not title.startswith("OPEN-"):
            continue
        for field in ("Dono:", "Bloqueia:"):
            if not re.search(rf"^{field}", chunk, re.M):
                errors.append(f"open-decisions.md [{title}]: falta '{field}'")
else:
    errors.append("docs/product-specs/open-decisions.md não existe")

# 6. formato de cada RN (por bloco '## RN-NNN') + unicidade do ID no catálogo
rn_ids_vistos = {}
for f in rn_files:
    t = f.read_text(encoding="utf-8")
    blocos = RN_BLOCO_RE.findall(t)
    if not blocos:
        errors.append(f"{rel(f)}: arquivo de RN sem bloco no formato '## RN-NNN'")
    for rid, corpo in blocos:
        rn_ids_vistos.setdefault(rid, []).append(rel(f))
        for sec in SECOES_RN:
            if sec not in corpo:
                errors.append(f"{rel(f)}: {rid} sem a seção obrigatória '{sec}'")
for rid, arquivos in rn_ids_vistos.items():
    if len(arquivos) > 1:
        errors.append(f"ID de RN duplicado: {rid} em {', '.join(arquivos)} — a sequência é única no catálogo")

# 6b. formato de cada ADR (numeração casa com o arquivo + seção Status)
for f in adr_files:
    t = f.read_text(encoding="utf-8")
    num = f.name[:3]
    if not re.search(rf"^#\s+ADR-{num}\b", t, re.M):
        errors.append(f"{rel(f)}: título deve começar com '# ADR-{num}:'")
    if not re.search(r"^##\s+Status", t, re.M):
        errors.append(f"{rel(f)}: ADR sem a seção '## Status'")

# 7. AGENTS.md é um mapa
agents = ROOT / "AGENTS.md"
if not agents.exists():
    errors.append("AGENTS.md não existe")
else:
    n = len(agents.read_text(encoding="utf-8").splitlines())
    if n > 150:
        errors.append(f"AGENTS.md tem {n} linhas (máximo 150 — deve permanecer um mapa conciso)")

# 8. exec-plans
for f in (ROOT / "docs" / "exec-plans" / "active").glob("*.md"):
    if not re.search(r"^#+\s*Evid[êe]ncias", f.read_text(encoding="utf-8"), re.M):
        errors.append(f"{rel(f)}: exec-plan ativo sem seção 'Evidências'")
for f in (ROOT / "docs" / "exec-plans" / "completed").glob("*.md"):
    m = re.search(r"^#+\s*Evid[êe]ncias\s*\n(.*)$", f.read_text(encoding="utf-8"), re.S | re.M)
    corpo = (m.group(1).strip() if m else "")
    if not corpo or corpo.startswith("("):
        errors.append(f"{rel(f)}: exec-plan concluído com seção 'Evidências' vazia")

# 10. pastas de framework de desenvolvimento não podem ser versionadas (ADR-004):
#     o artefato do kit é scratch; a verdade canônica vive em docs/. A lista é
#     mantida conforme os kits em uso — adicione a pasta do seu kit aqui e no .gitignore.
PASTAS_FRAMEWORK = {".specify", "specs", "memory", ".gsd", "gsd", ".superpowers"}
try:
    tracked = subprocess.run(
        ["git", "ls-files"], cwd=ROOT,
        capture_output=True, text=True, check=True).stdout.splitlines()
    poluidas = sorted({p.split("/", 1)[0] for p in tracked
                       if p.split("/", 1)[0] in PASTAS_FRAMEWORK})
    for d in poluidas:
        errors.append(f"pasta de framework '{d}/' está versionada — é scratch de "
                      f"ferramenta, não produto (ADR-004): remova do versionamento e "
                      f"aterrisse o resultado em docs/.")
except (OSError, subprocess.CalledProcessError):
    warnings.append("git indisponível — checagem de pasta de framework (ADR-004) pulada")

for w in warnings:
    print(f"aviso: {w}")
if errors:
    print("HARNESS QUEBRADO:")
    for e in errors:
        print(f"  - {e}")
    sys.exit(1)
print("harness ok")
