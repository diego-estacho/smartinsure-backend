---
id: ADR-026
title: Core sem dependências externas
status: accepted
tags: [dominio]
applies-to: ["src/*.Core/**"]
supersedes: []
evidence: []
---

# ADR-026: Core sem dependências externas

## Status

Aceito

## Decisão (normativa)

- O projeto de domínio (Core) NUNCA referencia pacote NuGet externo — apenas a BCL.
- O Core NUNCA referencia outro projeto da solution; é o nó mais interno do grafo.
- Necessidades de infraestrutura (persistência, mensageria, serviços externos) DEVEM ser expressas como interfaces no Core (`Abstractions/`) e implementadas fora.
- Entidades NUNCA carregam atributos de persistência ou serialização (EF, Bson, Json) — mapeamento vive na infraestrutura (ADR-033).
- A regra é travada por teste de arquitetura (ADR-052).

## Contexto

O domínio é o código de maior longevidade do sistema; cada dependência externa nele acopla as regras de negócio ao ciclo de vida de uma biblioteca. Com contratos no Core e implementações fora, trocar tecnologia de infraestrutura não toca o domínio.

## Consequências

Alguns tipos exigem definição própria no Core em vez de reusar tipos de biblioteca. Documentos de log/auditoria persistidos no Mongo são mapeados por configuração externa ou vivem como modelos da camada de infraestrutura — nunca forçam driver no Core.
