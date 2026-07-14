---
id: ADR-018
title: Documentação de API via OpenAPI + Scalar em ambientes internos
status: accepted
tags: [api, operacao]
applies-to: ["src/*.Api/**"]
supersedes: []
evidence: []
---

# ADR-018: Documentação de API via OpenAPI + Scalar em ambientes internos

## Status

Aceito

## Decisão (normativa)

- O contrato da API DEVE ser publicado via OpenAPI com UI Scalar.
- Scalar/OpenAPI DEVEM estar habilitados em desenvolvimento e QA; em produção DEVEM estar desligados (gate por ambiente, nunca comentado ou hardcoded).
- Toda rota DEVE contribuir com metadados de contrato (`WithName`, `WithSummary`, `Produces<T>` — ver ADR-009); rota sem contrato declarado é desvio.
- Schemas DEVEM usar ids únicos e estáveis para evitar colisão de tipos homônimos.

## Contexto

Scalar oferece a UI de contrato sobre o OpenAPI nativo do ASP.NET. Manter docs ligadas em produção amplia a superfície exposta sem necessidade — o contrato para consumo externo é distribuído pelos ambientes internos.

## Consequências

Consumidores internos têm contrato navegável em dev/QA. A produção não expõe metadados da API. O gate por ambiente precisa ser configuração explícita, verificável em code review.
