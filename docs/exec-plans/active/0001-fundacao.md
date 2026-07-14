# Exec-plan 0001 — Fundação

Status: não iniciado
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `docs/constitution.md`, `docs/PLANS.md`, `docs/QUALITY_SCORE.md`, `docs/SECURITY.md`, `docs/product-specs/glossario.md`, `docs/product-specs/open-decisions.md` e os ADRs em `docs/adr/`.

## Objetivo

Levar o repositório da fundação de harness ao produto, com o primeiro incremento vertical verificável em tela. **Este plano é esqueleto:** tudo que envolve regra de negócio ou arquitetura é preenchido junto com as tarefas, na abertura de cada fase — nada é cravado aqui de antemão.

## Tarefas

### Fase A — Ratificação (humanos, nenhum código antes disso)

- [ ] Ratificar as ADRs de harness (001–004) e a constituição com o time
- [ ] Ratificar glossário e enumerar a máquina de estados com a PO (OPEN-01)
- [ ] Commit inicial deste repositório

### Fase B — Scaffold e motor

- [ ] Definir a arquitetura do backend e do front (dono: arquitetura) e registrá-la como ADR — o scaffold segue essa decisão, não é cravado aqui
- [ ] Scaffold dos dois repositórios no layout de workspace (ADR-001/002), a partir deste repositório
- [ ] Contrato entre repos publicado no CI + geração de types no front com verificação de drift (ADR-001)
- [ ] CI com gates reais nos dois repos: ativar os steps do `azure-pipelines.yml` — zero `continue-on-error`, cobertura mínima de 80% como gate
- [ ] Testes de arquitetura das invariantes no gate de PR — as invariantes são definidas junto com a arquitetura
- [ ] Ponteiros por ferramenta de IA (`CLAUDE.md` e equivalentes → `AGENTS.md`), mantendo o harness agnóstico (ADR-003) — sem vendorar framework/skill por padrão
- [ ] Credenciais de banco read-only por papel para agentes — garantia no servidor (ADR-003)
- [ ] Catalogar com a PO as RNs das primeiras jornadas (processo em `docs/product-specs/regras-de-negocio/README.md`) e disponibilizar o gerador do mapa de rastreabilidade em `scripts/`
- [ ] Preencher a seção Comandos do AGENTS.md com os comandos reais

### Fase C — Primeira fatia vertical

- [ ] Uma jornada de negócio de ponta a ponta (tela → API → integração → status próprio) — a jornada, o escopo e a integração são definidos pela RN e pela arquitetura na abertura desta fase, não aqui
- [ ] E2E da jornada como gate de merge
- [ ] Relatório de rastreabilidade inicial: RNs catalogadas × cobertas por teste

## Critérios de aceite

- `python scripts/check-harness.py` verde no CI; nenhum step com `continue-on-error`.
- Fatia vertical demonstrável em tela, com E2E bloqueante.
- RNs das primeiras jornadas catalogadas e aprovadas pela PO, com IDs estáveis e mapa de rastreabilidade regenerável por script.

## Evidências

(preencher durante a execução: comandos rodados, resultados, links de PR, screenshots)
