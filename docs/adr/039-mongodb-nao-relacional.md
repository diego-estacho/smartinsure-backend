---
id: ADR-039
title: MongoDB restrito a dados não relacionais
status: accepted
tags: [dados]
applies-to: ["src/*.Infra.Data/**"]
supersedes: []
evidence: []
---

# ADR-039: MongoDB restrito a dados não relacionais

## Status

Aceito

## Decisão (normativa)

- MongoDB DEVE guardar exclusivamente dados não relacionais: logs de integração, payloads de request/response, auditoria de tarefas.
- Agregados de negócio NUNCA vivem no Mongo; o SQL Server é o único store transacional.
- O acesso DEVE passar por `IMongoRepository<T>` genérico (contrato no Core), com coleção nomeada pelo tipo do documento.
- Escrita no Mongo é best-effort observacional: falha de log NUNCA aborta a operação de negócio (capturar, registrar e seguir).
- `IMongoClient` DEVE ser registrado como Singleton; a connection vem de Options (ADR-053).

## Contexto

Payloads de integração e logs têm shape variável e volume alto — mal servidos por tabelas relacionais. Já os agregados exigem transação e consistência do SQL Server. A fronteira rígida evita o vazamento gradual de dados de negócio para o store sem transação.

## Consequências

Não há transação atômica entre SQL e Mongo — aceitável porque o Mongo só carrega dados observacionais. Consultas operacionais sobre payloads acontecem no Mongo sem impactar o banco transacional.
