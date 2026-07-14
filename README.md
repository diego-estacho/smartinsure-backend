# SmartInsure — Harness IA-First

Este é o repositório do backend do SmartInsure e carrega o harness IA-first do produto: a fundação documental e mecânica sobre a qual o produto é construído. Os princípios: docs versionados como fonte de verdade, `AGENTS.md` como mapa (não enciclopédia), entrevista antes de código, decisões registradas como ADRs em `docs/adr/` e verificação mecânica em vez de regra em prosa.

**Novo no time?** Comece pelo [manual de trabalho](docs/ONBOARDING.md) — o guia do primeiro dia.

## Conteúdo do repositório

- `AGENTS.md` — o mapa que todo agente e todo dev lê primeiro (~100 linhas, nunca mais que 150 — o lint garante).
- `ARCHITECTURE.md` — stub: arquitetura do backend está fora do escopo do harness por enquanto (dono: arquitetura); nasce depois, seguindo o padrão de ADR/doc daqui.
- `docs/` — fonte de verdade: sentido de produto (`PRODUCT_SENSE.md`), processo e exec-plans (`PLANS.md`), régua de qualidade (`QUALITY_SCORE.md`), confiabilidade (`RELIABILITY.md`, stub), **convenções do backend (`BACKEND.md`, stub — fora do escopo do harness por enquanto)**, segurança, `product-specs/` (glossário, RNs, decisões abertas), `constitution.md` (a constituição), `adr/` (ADRs), `generated/` (documentação derivada por script).
- Sem skills/frameworks vendorados no repo: o harness é agnóstico (ADR-003) — cada dev usa a ferramenta que preferir no próprio ambiente; o repo cobra o resultado, não a ferramenta.
- `scripts/check-harness.py` — o **componente central**: lint mecânico do próprio harness (links quebrados; referências a arquivos e IDs `ADR`/`OPEN`/`RN` em prosa; índices desatualizados; ADR sem status; decisão aberta sem dono; formato de RN; AGENTS.md acima do limite; exec-plan sem evidência). Roda no CI. É este script que diferencia o harness de documentação estática sem verificação.
- `scripts/doctor.py` — valida o ambiente local e o layout de workspace (repos irmãos lado a lado). Advisório, para o primeiro dia.
- `.github/CODEOWNERS` — roteia o review por especialidade (backend, PO, harness); handles preenchidos na criação do repo na organização.
- `azure-pipelines.yml` — CI que roda o lint hoje; os gates de build/teste entram no scaffold (Fase B), sem `continue-on-error`.

## Fora do escopo (intencionalmente)

Código de aplicação (entra na Fase B, depois da ratificação do harness), orquestrador de fases, markers de estado, skills genéricas extensas. O modo de trabalho é **1 dev + 1 agente interativo, humano no loop, verificação mecânica no CI** (ver [docs/PLANS.md](docs/PLANS.md)).

## Por onde começar

1. Leia o [AGENTS.md](AGENTS.md) de ponta a ponta (5 minutos).
2. Rode `python scripts/check-harness.py` — deve passar. Quebre um link em qualquer doc e execute novamente — a verificação deve falhar. Esse é o comportamento esperado.
3. Leia os ADRs em [docs/adr/](docs/adr/) — cada um com Status `Proposto`: são **propostas para o time ratificar**, não decisões tomadas.
4. Leia [docs/product-specs/open-decisions.md](docs/product-specs/open-decisions.md) — o que está bloqueado aguardando decisão humana (a maioria sob responsabilidade da PO).

## Topologia

Este repositório é *docs-first*: o harness (a camada de produto em `docs/`) vem antes do código; o .NET entra no scaffold da Fase B, no mesmo repo. A camada de produto é fonte única e vive só aqui — não há repositório separado de harness ([ADR-001](docs/adr/001-camada-de-produto-unica.md)); o `smartinsure-frontend` recebe apenas um AGENTS.md fino apontando para `../smartinsure-backend/docs/`.

## Próximos passos

1. Ratificar as ADRs de harness (001, 002, 003) e a constituição com o time.
2. Ratificar o glossário e enumerar a máquina de estados com a PO (OPEN-01).
3. Executar o [exec-plan 0001 — Fundação](docs/exec-plans/active/0001-fundacao.md).
