---
id: ADR-037
title: Escrita exclusivamente via change tracker
status: accepted
tags: [dados, seguranca]
applies-to: ["src/*.Infra.Data/Repositories/**"]
supersedes: []
evidence: []
---

# ADR-037: Escrita exclusivamente via change tracker

## Status

Aceito

## Decisão (normativa)

- Toda escrita DEVE passar por entidade rastreada + `SaveChanges` (via UnitOfWork).
- SQL cru de escrita (`ExecuteSqlRaw`, `ExecuteSqlInterpolated`, `ExecuteUpdate`, `ExecuteDelete`) NUNCA é usado em código de aplicação ou repositório.
- Operações em massa excepcionais (backfill, correção de dados) DEVEM ser migrations Flyway (ADR-041), nunca código da aplicação.

## Contexto

Escrita crua bypassa tudo que o modelo garante: invariantes das entidades ricas (ADR-027), auditoria automática (ADR-030), query filters de tenant (ADR-035) e domain events (ADR-028). O ganho de performance só é relevante em operações em massa — que pertencem ao repositório de migrations, onde são versionadas e aplicadas com controle.

## Consequências

Atualizações pontuais carregam o custo de materializar a entidade — aceito em troca das garantias do modelo. Necessidades legítimas de bulk têm caminho definido (migration), com revisão e trilha.
