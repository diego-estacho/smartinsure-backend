---
id: ADR-042
title: Versionamento de migration por timestamp puro
status: accepted
tags: [migrations]
applies-to: ["migrations/**"]
supersedes: []
evidence: []
---

# ADR-042: Versionamento de migration por timestamp puro

## Status

Aceito

## Decisão (normativa)

- Toda migration DEVE seguir o padrão `V{yyyyMMddHHmmss}__descricao-em-kebab-case.sql`.
- Prefixo sequencial (V001, V002...) NUNCA é usado — a versão é o timestamp de criação.
- `outOfOrder` DEVE ser `false` na configuração do Flyway E no comando do CI; migration com timestamp anterior à última aplicada é rejeitada (o autor a recria com timestamp novo).
- A descrição DEVE dizer a mudança em kebab-case, sem espaços, sem caracteres especiais.

## Contexto

Prefixos sequenciais colidem quando dois desenvolvedores criam migrations em paralelo, e a colisão força `outOfOrder=true` — que quebra a garantia de ordem de aplicação. Timestamp puro torna colisão improvável e mantém ordem cronológica natural com `outOfOrder=false`.

## Consequências

O nome do arquivo perde a legibilidade do número curto — compensada pela descrição obrigatória. Rebase de migration antiga não aplicada exige recriar com timestamp novo, o que é desejável: a ordem de aplicação nunca é ambígua.
