# Processo de trabalho e exec-plans

## Modo de trabalho

**1 dev + 1 agente interativo, humano no loop.** Sem orquestrador de pipeline, sem fases automatizadas com markers: contrato entre etapas descrito em prosa e verificado por LLM não é confiável — a abordagem confiável é o dev dirigindo o agente, com verificação mecânica no CI.

**Atuação fullstack por funcionalidade.** O dono da feature a leva de ponta a ponta — entrevista/RN → backend → contrato → front → E2E — nos dois repositórios, sob o mesmo `AB#NNNNN`. A especialização do time entra no **review** ([QUALITY_SCORE.md](QUALITY_SCORE.md)), não na divisão da tarefa. O harness foi desenhado para isso: glossário e RNs únicos valem nos dois repos, o AGENTS.md fino de cada repo fornece o mapa local em poucos minutos, e os types gerados do contrato impedem o lado front de divergir do back.

**Framework de desenvolvimento é escolha de cada dev** — kit de especificação, skill de entrevista, gerador de plano, ou nenhum. O harness não prescreve ferramenta; exige o resultado: convenções respeitadas, RN aprovada pela PO, ADR quando houver decisão difícil de reverter e gates verdes ([ADR-003](adr/003-agnostico-de-ia-e-framework.md)).

## Fluxo de uma feature

O fluxo completo abaixo vale para atividade que toca comportamento de negócio. Ajuste sem regra de negócio (ex.: cor, layout, texto já no glossário) vai direto aos passos 3–4 — implementar + evidência —, mantendo o vocabulário do glossário.

1. **RN como primeiro passo** — ao pegar uma atividade que toca comportamento de negócio, a regra é levantada e refinada **junto com a PO** e catalogada (aprovada) antes de qualquer código ([regras-de-negocio](product-specs/regras-de-negocio/README.md)). O catálogo cresce por atividade, incrementalmente — não há migração em massa nem fila de aprovação, e é assim que a PO deixa de ser gargalo. A ferramenta de entrevista é livre (ADR-003); o resultado obrigatório é a RN aprovada, mais um ADR quando houver decisão difícil de reverter.
2. **Exec-plan** para trabalho de mais de ~1 dia (seção abaixo).
3. Implementação em fatias verticais pequenas. PR pequeno, um assunto.
4. Evidência de verificação no PR: teste rodando; screenshot/gravação da jornada quando houver UI.

## Exec-plans

- Estrutura obrigatória (padrão em [exec-plans/active/0001-fundacao.md](exec-plans/active/0001-fundacao.md)): **Contexto obrigatório** (o que ler antes de executar), **Tarefas**, **Critérios de aceite** e **Evidências** — o lint reprova exec-plan ativo sem a seção de Evidências.
- Ciclo de vida: o plano nasce em `exec-plans/active/`; concluído (evidências preenchidas), move para `exec-plans/completed/` no mesmo PR que encerra o trabalho.
- Dívida aceita conscientemente vai para o [tech-debt-tracker](exec-plans/tech-debt-tracker.md) com dono e gatilho de revisão (princípio 9 da [constituição](constitution.md)).

## Automação e anti-entropia

- Automação nova só pela **regra das 3 ocorrências**: tarefa executada manualmente 3 vezes com custo relevante → automatiza-se o mínimo necessário — sempre agnóstico (script no repo em vez de feature da ferramenta).
- Cadência semanal de **doc-gardening**: um agente compara docs ↔ código e abre PRs pequenos de correção.
- Passada periódica de simplificação sobre as áreas tocadas na semana, aplicando o princípio 8 da [constituição](constitution.md): dois padrões nunca convivem.
