---
id: ADR-033
title: Mapeamento EF exclusivamente por Fluent API
status: accepted
tags: [dados]
applies-to: ["src/*.Infra.Data/Mappings/**", "src/*.Core/Entities/**"]
supersedes: []
evidence: []
---

# ADR-033: Mapeamento EF exclusivamente por Fluent API

## Status

Aceito

## Decisão (normativa)

- Toda configuração de persistência DEVE viver em `Mappings/{Entidade}Mapping : IEntityTypeConfiguration<T>`, carregada por `ApplyConfigurationsFromAssembly`.
- Entidades NUNCA carregam Data Annotations de persistência (`[Column]`, `[MaxLength]`, `[Table]`) nem de serialização.
- Cada entidade mapeada DEVE ter exatamente um arquivo de mapping, com o sufixo `Mapping`, na pasta `Mappings/` — nomes ou pastas alternativos NUNCA são usados.
- Tipos monetários DEVEM ter precisão explícita no mapping (`HasPrecision`); strings DEVEM ter tamanho máximo explícito.
- O mapping é a fonte única da verdade do schema esperado pelo modelo; divergência com a migration Flyway correspondente é bug.

## Contexto

Duas fontes de mapeamento (annotations no domínio + Fluent na infra) inevitavelmente divergem e violam a separação da ADR-026. Fluent API concentra tudo num lugar previsível, com acesso completo aos recursos do EF (conversões, filtros, índices).

## Consequências

O mapping de cada entidade é um arquivo achável por convenção. O domínio permanece limpo de preocupações de persistência. Todo campo novo exige toque no mapping — visível em review junto com a migration.
