# Régua de qualidade

A qualidade deste repositório é medida por gates mecânicos, não por opinião: trabalho que passa nos gates atende à régua. Gate que ainda não existe tem tarefa de criação no [exec-plan 0001](exec-plans/active/0001-fundacao.md) — e nasce bloqueante.

## Testes que importam

| Camada | O que cobre | Quando roda |
|---|---|---|
| E2E de jornada (Playwright) | jornadas do corretor de ponta a ponta (oferta → cotações → proposta → apólice) — vive no repo do front | **gate de merge** do front (jornadas afetadas) + suíte completa noturna contra ambiente dev |
| Regras de negócio | cada RN catalogada tem teste que carrega o ID dela; pontos de dinheiro (cálculo, transições de status, idempotência) exigem teste dedicado | gate de PR |

As camadas de teste específicas da arquitetura — invariantes estruturais e contratos de integração por seguradora — são definidas junto com a arquitetura, quando ela for documentada aqui.

## Regras inegociáveis

- **`continue-on-error` é proibido no pipeline.** Teste vermelho bloqueia merge. Teste que roda mas não bloqueia é pior que a ausência de teste: cria uma falsa sensação de segurança.
- Projeto de teste sem assert não entra no repo.
- **Cobertura mínima de 80% é gate de CI nos dois repositórios** — requisito de grandes seguradoras para se conectarem à plataforma; o pipeline reprova abaixo do piso.
- Cobertura é piso, não alvo: teste existe para verificar comportamento (RN, transição de status, contrato, caso de borda). Teste que apenas executa código para pontuar cobertura, sem asserção significativa, é reprovado em review — o percentual se atinge testando risco, não inflando números.
- Bug de produção ganha teste de regressão pelo fluxo adversarial: teste que falha → fix → teste passa → **reverter o fix e confirmar que volta a falhar** → reaplicar.
- Teste de RN carrega o ID (`[Trait("RuleId", "RN-NNN")]` / `describe('RN-...')`); o mapa de rastreabilidade é **derivado por script** ([generated/](generated/README.md)), nunca declarado à mão.

## Gates de CI

- **PR (minutos):** build · testes de RN e de pontos de dinheiro · cobertura ≥ 80% · lint + typecheck · `python scripts/check-harness.py`.
- **RN vinculada ao código (gate da Fase B, quando houver código):** PR que altera comportamento de negócio precisa de ao menos um teste carregando um `RuleId` de RN existente no catálogo — código de negócio sem RN vinculada reprova. É o que torna mecanicamente impossível "pular a RN" no merge, seja qual for a ferramenta usada.
- **Merge em `master`:** gates de PR + E2E das jornadas afetadas.
- **Noturno:** suíte E2E completa. O noturno complementa o gate; não o substitui. Doc-gardening tem cadência própria, semanal ([PLANS.md](PLANS.md)).
- Contrato entre repos: o backend publica `openapi.json` no CI; o front gera os types a partir dele e o CI do front falha se divergirem (ADR-001). Nenhum type de API mantido à mão no front.

## Review humano

Obrigatório via CODEOWNERS (`.github/CODEOWNERS`, com os handles reais preenchidos na criação do repo na organização) — quem escreve é o dono da feature (fullstack); quem revisa é o especialista: pontos de dinheiro (emissão/pagamento/status) e integrações de seguradora → Thiago ou Cleber (backend); front/design system → Diego; glossário e catálogo de RNs → PO; docs do harness → Thiago, Cleber e Diego (todos).
