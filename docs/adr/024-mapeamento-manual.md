---
id: ADR-024
title: Mapeamento DTO↔entidade manual, sem biblioteca
status: accepted
tags: [application]
applies-to: ["src/*.Application.UseCase/**"]
supersedes: []
evidence: []
---

# ADR-024: Mapeamento DTO↔entidade manual, sem biblioteca

## Status

Aceito

## Decisão (normativa)

- Conversões entre requests/responses e entidades DEVEM ser explícitas: construtores de entidade, object initializers ou mappers estáticos por feature.
- Bibliotecas de mapeamento (AutoMapper, Mapster ou similares) NUNCA são adicionadas à solution.
- Quando a conversão passa de trivial, ela DEVE ser extraída para um mapper estático nomeado da feature (ex.: `{Feature}ResponseMapper.ToResponse(...)`), testável isoladamente.
- Construção de entidade a partir de request DEVE passar pelo construtor/factory da entidade (ADR-027), preservando invariantes — NUNCA por atribuição direta campo a campo em entidade já construída.

## Contexto

Mapeamento por reflection esconde as conversões do compilador: campo novo não mapeado falha em runtime (ou silenciosamente) em vez de quebrar o build. Mapeamento manual é verboso, porém explícito, depurável e refatorável com segurança de tipos.

## Consequências

Cada campo novo exige toque no mapper correspondente — o compilador e o code review enxergam a mudança. Não há custo de configuração/perfil de mapper nem surpresas de convenção implícita.
