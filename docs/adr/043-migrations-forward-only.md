---
id: ADR-043
title: Migrations imutáveis (forward-only)
status: accepted
tags: [migrations]
applies-to: ["migrations/**"]
supersedes: []
evidence: []
---

# ADR-043: Migrations imutáveis (forward-only)

## Status

Aceito

## Decisão (normativa)

- Migration aplicada em qualquer ambiente NUNCA é editada ou removida.
- Correção de migration errada DEVE ser uma nova migration (revert explícito e/ou re-aplicação corrigida).
- Scripts undo/rollback automáticos NUNCA são mantidos; o caminho é sempre para frente.
- Migration DEVE ser segura para reexecução conceitual: guards de existência (`IF OBJECT_ID ... IS NULL`) em DDL onde aplicável.

## Contexto

Editar migration aplicada dessincroniza o checksum do Flyway e cria bancos com histórias divergentes entre ambientes. O modelo forward-only garante que a sequência de migrations reproduz qualquer ambiente do zero e que o histórico conta a verdade, inclusive dos erros.

## Consequências

Erros de schema deixam rastro (migration errada + correção) em vez de sumirem — trilha de auditoria completa. O repositório de migrations só cresce; o tamanho é irrelevante frente à reprodutibilidade.
