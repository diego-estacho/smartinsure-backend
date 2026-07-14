---
id: ADR-029
title: Identidade de entidade com chave única UUIDv7
status: accepted
tags: [dominio, dados]
applies-to: ["src/*.Core/Entities/**", "src/*.Infra.Data/Mappings/**"]
supersedes: []
evidence: []
---

# ADR-029: Identidade de entidade com chave única UUIDv7

## Status

Aceito

## Decisão (normativa)

- Toda entidade DEVE ter uma única chave de identidade: `Guid Id`, gerado por `Guid.CreateVersion7()`.
- O Id DEVE ser gerado pela aplicação na construção da entidade; NUNCA pelo banco (sem IDENTITY/sequence para chave).
- O mesmo Id é usado internamente (PK, FKs) e externamente (rotas, payloads); identidade dupla (id interno + id de exposição) NUNCA é criada.
- Chaves compostas só em tabelas de junção; entidades de negócio NUNCA usam chave composta.

## Contexto

UUIDv7 é ordenável temporalmente, o que elimina a fragmentação de índice que penalizava GUIDs aleatórios como chave clusterizada, e dispensa a dupla identidade (int interno + Guid externo) que obriga tradução constante entre borda e persistência. A geração na aplicação permite conhecer o Id antes do commit (eventos, logs, retorno 202).

## Consequências

Chaves de 16 bytes ocupam mais que int em índices — aceito em troca de identidade única ponta a ponta. Ids são não sequenciais para observador externo, sem vazamento de volume de negócio.
