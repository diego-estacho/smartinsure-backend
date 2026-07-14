---
id: ADR-034
title: FKs com DeleteBehavior.Restrict por convenção global
status: accepted
tags: [dados]
applies-to: ["src/*.Infra.Data/Context/**"]
supersedes: []
evidence: []
---

# ADR-034: FKs com DeleteBehavior.Restrict por convenção global

## Status

Aceito

## Decisão (normativa)

- Toda foreign key DEVE usar `DeleteBehavior.Restrict`, aplicado por convenção global no `OnModelCreating` (loop sobre todas as FKs do modelo).
- Cascade delete NUNCA é habilitado, nem caso a caso.
- Exclusão de agregado com dependentes DEVE ser tratada como operação de domínio explícita (método que remove/arquiva dependentes) ou negada com `ConflictException` (ADR-022).

## Contexto

Cascade delete apaga dados em cadeia de forma invisível para o código de domínio — uma exclusão aparentemente local destrói histórico. Restrict global força cada exclusão a declarar o que acontece com os dependentes, e o banco protege contra o esquecimento.

## Consequências

Exclusões compostas exigem código explícito (ou viram soft delete/arquivamento por decisão de produto). Erros de FK em exclusão indevida aparecem como exceção clara em vez de perda silenciosa de dados.
