---
id: ADR-052
title: Testes de arquitetura como gate permanente
status: accepted
tags: [testes, dominio]
applies-to: ["src/*.Tests/**"]
supersedes: []
evidence: []
---

# ADR-052: Testes de arquitetura como gate permanente

## Status

Aceito

## Decisão (normativa)

- As regras de dependência entre camadas DEVEM ser travadas por testes NetArchTest no projeto de testes unitários, rodando no gate de PR (ADR-008).
- No mínimo, DEVEM estar travados: Core sem referências externas (ADR-026); Application sem referência à infraestrutura de dados (ADR-023); herança obrigatória da base de auditoria (ADR-030); convenções de naming estruturais (sufixos Endpoint/Repository/UseCase/Mapping).
- `Skip` permanente em teste de arquitetura NUNCA é aceito; exceção temporária exige issue rastreada e prazo.
- Regra arquitetural nova DEVE nascer com seu teste correspondente no mesmo PR.

## Contexto

Regra de arquitetura sem verificação automática degrada por acumulação de exceções pequenas — cada desvio individual parece inofensivo. O teste transforma a regra em falha de build, deslocando a discussão para antes do merge.

## Consequências

Violações aparecem no PR que as introduz, com custo de correção mínimo. O conjunto de testes de arquitetura funciona como a especificação executável destas ADRs.
