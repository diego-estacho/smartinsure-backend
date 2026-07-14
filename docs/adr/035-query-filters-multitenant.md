---
id: ADR-035
title: Isolamento multi-tenant por Global Query Filters
status: accepted
tags: [dados, seguranca]
applies-to: ["src/*.Infra.Data/**", "src/*.Infra.CrossCutting/Identity/**"]
supersedes: []
evidence: []
---

# ADR-035: Isolamento multi-tenant por Global Query Filters

## Status

Aceito

## Decisão (normativa)

- O escopo de tenant DEVE ser aplicado por Global Query Filters no DbContext, sobre toda entidade que carrega o identificador de tenant.
- Os filtros DEVEM ser alimentados por accessors de contexto (ex.: `ICurrentBrokerAccessor`) injetados como anuláveis: accessor nulo = execução de sistema (Functions), com bypass documentado.
- Código de consulta NUNCA reimplementa o filtro de tenant em `Where` manual como mecanismo principal; o filtro global é a garantia.
- `IgnoreQueryFilters()` NUNCA é usado em código de aplicação; uso em infraestrutura exige justificativa em comentário e revisão.
- Entidade nova com dado de tenant DEVE entrar no filtro global no mesmo PR que a cria.

## Contexto

Filtro de tenant por `Where` manual falha por omissão — basta um esquecimento para vazar dados entre tenants. O filtro global inverte o default: a consulta nasce escopada e o bypass é que precisa ser explícito.

## Consequências

Recurso de outro tenant é invisível (vira 404 pela ADR-012 — sem oráculo de existência). Execuções de sistema enxergam tudo por design, o que exige disciplina nos pontos de bypass. O filtro compõe com o plano de query em toda consulta — custo aceito.
