---
id: ADR-030
title: Auditoria de entidade obrigatória e automática
status: accepted
tags: [dominio, dados]
applies-to: ["src/*.Core/Entities/**", "src/*.Infra.Data/**"]
supersedes: []
evidence: []
---

# ADR-030: Auditoria de entidade obrigatória e automática

## Status

Aceito

## Decisão (normativa)

- Toda entidade DEVE herdar a entidade base de auditoria com `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`.
- Os campos de auditoria DEVEM ser preenchidos exclusivamente por um `SaveChangesInterceptor` do EF; código de aplicação NUNCA os atribui manualmente.
- Timestamps DEVEM ser UTC (`DateTime.UtcNow`); `DateTime.Now` NUNCA é usado em nenhum ponto da solution.
- A herança obrigatória da base DEVE ser travada por teste de arquitetura (ADR-052).

## Contexto

Auditoria preenchida manualmente diverge: cada ponto de escrita decide se preenche, com qual relógio e qual usuário. Interceptor único garante consistência total e remove a preocupação do código de negócio. UTC uniforme elimina ambiguidade de fuso na persistência (exibição converte na borda).

## Consequências

Todas as tabelas carregam as quatro colunas — inclusive onde a auditoria é pouco usada, custo aceito pela uniformidade. O interceptor precisa do usuário corrente acessível (via accessor de identidade) inclusive em execuções de sistema (Functions), que auditam como usuário-sistema.
