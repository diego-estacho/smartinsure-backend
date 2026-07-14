# Manual de trabalho — SmartInsure

Guia do primeiro dia: **como se trabalha neste repositório.** Se você acabou de entrar no time, leia isto uma vez de ponta a ponta (~10 minutos) e volte quando precisar. Este manual **aponta** para as fontes de verdade; não as substitui — em conflito, valem os arquivos versionados que ele referencia.

## 1. O que é este repositório

É o backend do SmartInsure e o **harness** IA-first do produto (a camada de docs que orienta pessoas e agentes). Três ideias fundam tudo:

- **Os docs versionados são a verdade** — não o chat, não a memória da IA. Em conflito, o arquivo vence.
- **A ferramenta de IA e o framework de desenvolvimento são livres** — Claude, speckit, grill-me, ou nenhum. O harness cobra o **resultado**, não a ferramenta ([ADR-003](adr/003-agnostico-de-ia-e-framework.md)).
- **A fronteira que importa é testada** — o CI reprova o que fere as regras; regra que só existe em prosa acaba violada.

## 2. Primeiros 5 minutos

1. Leia o [`AGENTS.md`](../AGENTS.md) de ponta a ponta — é o **mapa** (aponta para tudo; ~100 linhas).
2. Leia a [constituição](constitution.md) — os princípios inegociáveis.
3. Rode as checagens locais:

```
python scripts/doctor.py         # confere seu ambiente e o workspace
python scripts/check-harness.py  # lint dos docs — tem que passar
```

Se o `check-harness.py` reprovar, algum doc está inconsistente — conserte antes de seguir.

## 3. Como o trabalho flui

O processo completo está no [`PLANS.md`](PLANS.md). Ao pegar uma atividade, faça a **triagem**: toca comportamento de negócio (dinheiro, status, permissão, cálculo, regra) ou precisa de contrato novo/alterado?

- **Sim** (ex.: gerenciar usuários, cotar, emitir) → é **cross-repo**: começa pela **RN** aqui no backend (seção 4), antes do código; o front consome. Os PRs dos dois repos são linkados pelo mesmo `AB#NNNNN`.
- **Não** (ex.: componente novo, página sem backend, ajuste de layout) → é **front-only**: não passa por aqui — é implementada no `smartinsure-frontend`, mantendo o vocabulário do glossário.

Trabalho de mais de ~1 dia ganha um **exec-plan**: no backend ([`docs/exec-plans/`](exec-plans/)) quando é cross-repo; no próprio front (`docs/exec-plans/` de lá) quando é front-only.

## 4. Sua primeira tarefa de negócio, passo a passo

Exemplo: "fazer o fluxo de usuários".

**Passo 1 — glossário.** Se os termos da jornada ("Usuário", "Permissão", "Papel") não estão no [glossário](product-specs/glossario.md), entram primeiro, aprovados pela PO. É incremental: só os termos desta jornada, não o glossário inteiro.

**Passo 2 — a RN (regra de negócio), antes de qualquer código:**

1. Copie o template [`_template.md`](product-specs/regras-de-negocio/_template.md) para o arquivo da jornada: `docs/product-specs/regras-de-negocio/usuarios.md`.
2. Escreva uma seção `##` por regra, com o ID `RN-NNN` (sequência única do catálogo) e os 4 blocos: **Descrição, Pré-condições, Critério de aceitação, Casos limite**. Linguagem de negócio, termos do glossário, sem caminho de código.
3. **A PO aprova.** Só então a RN é implementável. Processo completo no [README do catálogo](product-specs/regras-de-negocio/README.md).

**Passo 3 — implementar (framework livre).** Só agora entra o kit que você preferir (speckit, etc.) para construir. Ele **consome** a RN; não a substitui. A pasta de trabalho do kit **não é versionada** ([ADR-004](adr/004-fronteira-do-artefato-de-framework.md)) — o que fica no repo é a RN, o código e o teste.

**Passo 4 — evidência no PR.** Cada teste que cobre uma RN carrega o ID dela, no formato `RN-NNN`. No PR vai a evidência: teste rodando; print/gravação quando houver tela.

## 5. Onde as coisas moram

| Preciso de… | Está em… |
|---|---|
| O mapa (ler primeiro) | [`AGENTS.md`](../AGENTS.md) |
| Princípios inegociáveis | [`constitution.md`](constitution.md) |
| Decisões registradas (ADRs) | [`adr/`](adr/) |
| Vocabulário do produto | [glossário](product-specs/glossario.md) |
| Catálogo de regras de negócio | [regras-de-negocio](product-specs/regras-de-negocio/README.md) |
| O que está bloqueado esperando decisão | [open-decisions](product-specs/open-decisions.md) |
| Processo de trabalho | [`PLANS.md`](PLANS.md) |
| Régua de qualidade / gates | [`QUALITY_SCORE.md`](QUALITY_SCORE.md) |
| Regras de segurança | [`SECURITY.md`](SECURITY.md) |

## 6. O que reprova seu PR (não seja pego de surpresa)

Detalhes no [`QUALITY_SCORE.md`](QUALITY_SCORE.md) e no [`SECURITY.md`](SECURITY.md):

- `check-harness.py` vermelho (docs inconsistentes).
- Cobertura de testes abaixo de 80%.
- `continue-on-error` no pipeline — proibido.
- Segredo/credencial versionada — nunca, nem em teste.
- Dinheiro ou permissão validados só no cliente — a garantia é no servidor.

## 7. Convenções rápidas

Do [`AGENTS.md`](../AGENTS.md):

- Branch: `ab-NNNNN-slug-curto` (sem `#`); o PBI é vinculado como `AB#NNNNN` no commit/PR.
- Idioma: termos de domínio em pt-BR (iguais ao glossário); commits em pt-BR (Conventional Commits).
- PR pequeno, um assunto.

## 8. Checklist antes de abrir o PR

- [ ] Se toca negócio: RN catalogada e aprovada pela PO, com ID.
- [ ] Termos novos já estão no glossário (aprovados pela PO).
- [ ] Testes cobrindo a RN, carregando o ID; cobertura ≥ 80%.
- [ ] `check-harness.py` verde.
- [ ] Evidência no PR (teste rodando; tela quando houver UI).
- [ ] Docs/RN/ADR atualizados no mesmo PR, se o comportamento ou uma decisão mudou.

Dúvida sobre uma regra que não existe? **Pare e registre em [open-decisions](product-specs/open-decisions.md)** com dono sugerido — nunca invente regra de negócio.
