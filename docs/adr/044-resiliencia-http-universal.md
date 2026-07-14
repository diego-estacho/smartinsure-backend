---
id: ADR-044
title: Resiliência HTTP universal (Refit + HttpClientFactory + Standard Resilience)
status: accepted
tags: [integracoes]
applies-to: ["src/*.Integration/**", "src/*.Providers.*/**"]
supersedes: []
evidence: []
---

# ADR-044: Resiliência HTTP universal (Refit + HttpClientFactory + Standard Resilience)

## Status

Aceito

## Decisão (normativa)

- Todo client HTTP externo DEVE ser uma interface Refit registrada via HttpClientFactory.
- Todo client DEVE ter `AddStandardResilienceHandler` (retry exponencial com jitter, circuit breaker, timeout por tentativa e total) com parâmetros calibrados ao SLA do destino.
- `new HttpClient()` ou `HttpClient` injetado sem factory NUNCA acontece.
- Retry DEVE respeitar a ADR-050: só para operações idempotentes; chamadas não idempotentes configuram retry zero.
- Headers de autenticação do parceiro DEVEM ser configurados no registro do client, nunca espalhados por chamada.

## Contexto

Dependência externa sem timeout/retry/circuit breaker transforma instabilidade do parceiro em indisponibilidade própria (threads presas, cascata de falha). O handler padrão da plataforma dá as três proteções com uma linha, tornando a resiliência o default e não um esforço por integração.

## Consequências

Toda integração nova nasce protegida — o parâmetro é revisado, não a existência da proteção. Circuit breaker aberto degrada rápido e explícito (exceção mapeável) em vez de travar requests. A calibração por destino exige conhecer o SLA de cada parceiro.
