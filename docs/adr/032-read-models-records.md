---
id: ADR-032
title: Read-models como records no domínio, retornados pelos repositórios
status: accepted
tags: [dominio, dados]
applies-to: ["src/*.Core/Abstractions/**", "src/*.Infra.Data/Repositories/**"]
supersedes: []
evidence: []
---

# ADR-032: Read-models como records no domínio, retornados pelos repositórios

## Status

Aceito

## Decisão (normativa)

- Consultas de leitura DEVEM projetar direto para DTOs `sealed record` (read-models) definidos em `Core/Abstractions/Repositories/Dtos/{Entidade}Dtos/` — uma subpasta por entidade.
- Entidades EF NUNCA atravessam a borda da API; response de endpoint carrega read-model ou response próprio, nunca a entidade.
- A projeção (`Select`) DEVE acontecer na query do repositório, materializando apenas as colunas do read-model.
- Read-model é imutável (`sealed record`) e NUNCA contém comportamento de domínio.

## Contexto

Retornar entidades para a borda vaza o modelo de persistência (navegações, campos internos, ciclos de serialização) e materializa colunas desnecessárias. Projeção no repositório resolve os dois problemas e documenta o contrato de leitura de cada consulta.

## Consequências

Cada consulta de listagem/detalhe tem seu record explícito — mais tipos, cada um dizendo exatamente o que a tela consome. Mudanças na entidade não quebram contratos de leitura silenciosamente (o compilador aponta as projeções afetadas).
