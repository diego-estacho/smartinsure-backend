---
id: ADR-038
title: Leituras com AsNoTracking e projeção direta
status: accepted
tags: [dados]
applies-to: ["src/*.Infra.Data/Repositories/**"]
supersedes: []
evidence: []
---

# ADR-038: Leituras com AsNoTracking e projeção direta

## Status

Aceito

## Decisão (normativa)

- Toda consulta de leitura (sem intenção de modificar) DEVE usar `AsNoTracking()`.
- Consultas de leitura DEVEM projetar direto para o read-model (`Select` → record da ADR-032); materializar a entidade completa para depois converter NUNCA é feito em listagens.
- Entidade só é materializada rastreada quando a operação vai modificá-la e persistir.
- Listagens DEVEM paginar no banco (`Skip/Take` a partir de `PagedRequest`); paginação em memória NUNCA é feita.

## Contexto

Tracking tem custo de memória e CPU proporcionais ao número de entidades materializadas e só serve para escrita. Em leitura, projeção sem tracking materializa apenas as colunas necessárias e elimina o overhead do change tracker.

## Consequências

A intenção da consulta (ler vs modificar) fica explícita no código do repositório. Consultas de leitura não seguram referência de entidade — impossível "editar por acidente" um resultado de listagem.
