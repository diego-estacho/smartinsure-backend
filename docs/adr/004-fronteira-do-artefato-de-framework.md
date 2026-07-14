# ADR-004: Fronteira do artefato de framework de desenvolvimento

## Status
Proposto (ratificação na Fase A)

## Contexto
O [ADR-003](003-agnostico-de-ia-e-framework.md) garante que cada dev use o framework de desenvolvimento que preferir (speckit, gsd, superpowers, grill-me, ou nenhum). Mas cada framework cria as próprias pastas e artefatos (`.specify/`, `specs/`, `memory/constitution.md`, planos, etc.), no formato dele. Sem uma fronteira, aparecem dois problemas:

1. **Poluição do repositório** — cada kit versiona sua árvore de arquivos e o repo vira uma colcha de retalhos de ferramentas.
2. **Fonte de verdade dupla** — o `spec.md` de um kit, o plano de outro e a constituição de um terceiro sobrepõem em conteúdo o glossário, as RNs, a constituição e os ADRs canônicos. Duas cópias divergem, e a IA (e o dev) se perde sobre qual regra de negócio vale.

Opções consideradas:
1. Deixar cada framework versionar suas pastas — rejeitado (é exatamente a poluição e o drift acima).
2. Padronizar um único framework para todos — rejeitado (contraria o ADR-003).
3. Tratar o artefato de framework como scratch efêmero e exigir que o resultado seja destilado nos docs canônicos.

## Decisão
O artefato nativo de qualquer framework de desenvolvimento é **área de trabalho efêmera, não versionada**. Ele é insumo para produzir o documento canônico — nunca uma verdade concorrente.

- A **verdade canônica** vive só em `docs/` (glossário, RNs, constituição, ADRs, decisões abertas, exec-plans) + `AGENTS.md`/`ARCHITECTURE.md` na raiz. Saída de framework precisa ser **aterrissada** nesses artefatos, no formato dos templates, para virar verdade.
- As pastas de trabalho de framework ficam no `.gitignore` (lista mantida conforme os kits em uso).
- **Gate mecânico:** o `check-harness.py` reprova o PR se qualquer pasta de framework conhecida estiver versionada — a fronteira é testada, não confiada à prosa (constituição, princípio 3).

## Consequências
- Trocar de framework não deixa cicatriz no repo; o conhecimento fica nos docs canônicos, não na ferramenta.
- Um kit novo entra adicionando sua pasta ao `.gitignore` e à lista do lint.
- O dev continua livre para usar o kit que quiser localmente (ADR-003); só não versiona o scratch dele.
- A constituição de um kit (ex.: `memory/constitution.md` do speckit) não coexiste com a nossa `docs/constitution.md` — o kit aponta para a canônica.
