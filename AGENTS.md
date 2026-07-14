# AGENTS.md — SmartInsure

Este arquivo é o mapa do harness. Todo agente (e todo dev) lê este arquivo antes de alterar código, banco, configuração ou documentação. Ele aponta para as fontes de verdade — não as duplica.

## Fonte de verdade (ordem de precedência)

1. [docs/product-specs/glossario.md](docs/product-specs/glossario.md) — vocabulário canônico. Nenhum nome de entidade, rota, tela ou status nasce fora dele.
2. [docs/product-specs/regras-de-negocio/](docs/product-specs/regras-de-negocio/README.md) — catálogo de RNs com IDs estáveis (fonte única de regra de negócio).
3. [docs/product-specs/open-decisions.md](docs/product-specs/open-decisions.md) — decisões pendentes com dono. O que está aberto aqui NÃO é implementável.
4. [ARCHITECTURE.md](ARCHITECTURE.md) — a definir; arquitetura do backend está fora do escopo do harness por enquanto (dono: arquitetura).
5. [docs/PRODUCT_SENSE.md](docs/PRODUCT_SENSE.md) — como pensar o produto nas decisões do dia a dia.
6. [docs/PLANS.md](docs/PLANS.md) — processo de trabalho, fluxo de feature e exec-plans.
7. [docs/QUALITY_SCORE.md](docs/QUALITY_SCORE.md) — régua de qualidade: testes, cobertura e gates de CI.
8. [docs/RELIABILITY.md](docs/RELIABILITY.md) — a definir; confiabilidade operacional está fora do escopo do harness por enquanto (dono: arquitetura).
9. [docs/BACKEND.md](docs/BACKEND.md) — a definir; convenções do backend (.NET) estão fora do escopo do harness por enquanto (dono: arquitetura).
10. [docs/SECURITY.md](docs/SECURITY.md) — regras obrigatórias de segurança e dados.
11. [docs/constitution.md](docs/constitution.md) — a constituição do harness; os ADRs vivem em [docs/adr/](docs/adr/). Decisão registrada só muda com novo ADR.
12. [docs/exec-plans/](docs/exec-plans/) — planos de execução do trabalho em andamento.

Conflito entre chat, memória e arquivos: **prevalecem os arquivos versionados**. Arquivo ambíguo ou regra inexistente: **pare e registre a pergunta em open-decisions.md** com dono sugerido. Nunca invente regra de negócio.

## Regras de trabalho

- Vocabulário: usar somente os termos do glossário, no código e na UI. Termo novo entra primeiro no glossário (aprovação da PO), depois no código.
- Pontos de dinheiro (emissão, pagamento, transição de status, cálculo) exigem review humano no PR e teste de domínio dedicado.
- Menor incremento vertical verificável; sem refactor amplo misturado a feature.
- Atividade que toca comportamento de negócio começa pela RN: a regra é levantada e refinada junto com a PO e catalogada (aprovada) ANTES do código — é o primeiro passo, não uma fila. Evidência de verificação vai no PR.
- Mudança de banco só por migration versionada, no repositório dedicado `DBMigrations` (padrão da empresa) — migrations nunca vivem neste repo.
- Cobertura de testes mínima de 80% é gate de CI; teste sem asserção de comportamento não conta (ver [QUALITY_SCORE.md](docs/QUALITY_SCORE.md)).
- Framework de desenvolvimento é livre (speckit, grill-me, ou nenhum): o harness valida o **resultado** — documento no padrão dos templates, RN aprovada, gates verdes —, nunca a ferramenta. É o que o torna plug-and-play (ADR-003).
- Pasta de trabalho de framework não é versionada; o resultado é aterrissado nos docs canônicos (ADR-004). Kit novo entra no `.gitignore` e na lista do lint.
- Segredo nunca em arquivo versionado (ver SECURITY.md).
- Antes de criar abstração nova, procurar padrão existente no repo.

## Fluxo por tarefa

1. Ler este arquivo + os docs relevantes ao tema; confirmar que nenhuma dependência está aberta em open-decisions.md.
2. **Se a atividade toca comportamento de negócio, o primeiro passo é a RN** — levantar/refinar a regra junto com a PO e catalogá-la (aprovada) antes de escrever código (processo em [regras-de-negocio](docs/product-specs/regras-de-negocio/README.md); ferramenta de entrevista livre — ADR-003, resultado obrigatório). Ajuste sem regra de negócio (ex.: layout, cor, texto já no glossário) pula os passos 2–3 e vai direto ao passo 4 — mantendo o vocabulário do glossário e a evidência no PR.
3. Decisão difícil de reverter vira ADR no mesmo passo.
4. Implementar o menor incremento vertical.
5. Rodar lint, typecheck, testes e build.
6. Evidência de verificação no PR (teste rodando; screenshot/gravação quando houver UI). Trabalho de mais de ~1 dia tem exec-plan, com a seção Evidências preenchida ao concluir.
7. Atualizar docs/RN/ADR quando comportamento ou decisão mudar — no MESMO PR.

## Convenções

- Branch: `ab-NNNNN-slug-curto` — sem `#` (MAX_PATH no Windows, ADR-002). O vínculo com o PBI vai no commit/PR como `AB#NNNNN` (integração Azure Boards).
- Worktrees nativas do git em raiz curta, com os repos como irmãos sob a pasta da atividade para os ponteiros de workspace resolverem: `git worktree add C:\wt\ab-NNNNN\smartinsure-backend` (e o análogo no front). ADR-002.
- Idioma: termos de domínio em pt-BR (idênticos ao glossário); código utilitário em inglês; UI, mensagens e commits em pt-BR (Conventional Commits).
- Workspace: repos clonados lado a lado (`C:\src\smart\smartinsure-backend` e `C:\src\smart\smartinsure-frontend`); o front referencia a camada de produto via `../smartinsure-backend/docs/` (ADR-001).
- Ferramenta de IA: o harness é agnóstico (ADR-003). O ponto de entrada é este AGENTS.md; `CLAUDE.md` e equivalentes são apenas ponteiros para ele.
- PR pequeno, um assunto. Mudança cross-repo: backend primeiro (contrato publicado), depois front — PRs linkados pelo mesmo `AB#NNNNN` (ADR-001).

## Comandos

Preenchidos no scaffold (Fase B do exec-plan 0001) — até lá, não assumir scripts inexistentes.

```
python scripts/doctor.py          # valida o ambiente e o layout de workspace
python scripts/check-harness.py   # lint do próprio harness
```
