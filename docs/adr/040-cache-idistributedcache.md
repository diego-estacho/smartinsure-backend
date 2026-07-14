---
id: ADR-040
title: Cache via abstração distribuída (IDistributedCache)
status: accepted
tags: [dados, operacao]
applies-to: ["src/**"]
supersedes: []
evidence: []
---

# ADR-040: Cache via abstração distribuída (IDistributedCache)

## Status

Aceito

## Decisão (normativa)

- Todo cache de aplicação DEVE usar `IDistributedCache` (`Microsoft.Extensions.Caching.Distributed`).
- A implementação inicial é `MemoryDistributedCache`; a troca por store distribuído (ex.: Redis) NUNCA pode exigir mudança em código consumidor — só em registro/configuração.
- `IMemoryCache` NUNCA é injetado diretamente em código de aplicação.
- Chaves de cache DEVEM ser constantes centralizadas (classe de constantes, ADR-053) com convenção de prefixo por área; toda entrada DEVE ter expiração explícita.

## Contexto

Começar com cache em memória atende o volume inicial, mas acoplar o código a `IMemoryCache` forçaria reescrita quando houver múltiplas instâncias. A abstração distribuída custa apenas serialização dos valores e deixa o caminho de escala aberto por configuração.

## Consequências

Valores cacheados precisam ser serializáveis desde o primeiro dia. Enquanto a implementação for em memória, o cache não é compartilhado entre instâncias — limitação conhecida e aceita até a troca de store.
