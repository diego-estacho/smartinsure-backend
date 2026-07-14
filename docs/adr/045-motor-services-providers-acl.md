---
id: ADR-045
title: Motor multi-fornecedor — contrato em Services, adaptador em Providers com ACL
status: accepted
tags: [integracoes, dominio]
applies-to: ["src/*.Services.*/**", "src/*.Providers.*/**"]
supersedes: []
evidence: []
---

# ADR-045: Motor multi-fornecedor — contrato em Services, adaptador em Providers com ACL

## Status

Aceito

## Decisão (normativa)

- O projeto `Services.<Motor>` DEVE definir o contrato do motor (`IProvider`, contexts, results) e resolver a implementação por tipo de engine configurado (Strategy); ele NUNCA faz I/O externo nem referencia pacotes HTTP.
- Cada fornecedor DEVE ser um projeto `Providers.<Fornecedor>` que implementa o contrato do motor: interfaces Refit, factory de client e mappers.
- Os mappers do provider são camada anticorrupção (ACL): o modelo do parceiro (requests/responses do fornecedor) NUNCA vaza para fora do projeto do provider — o provider fala com o resto do sistema só nos tipos do contrato do motor.
- A Application e o domínio NUNCA referenciam projetos de provider; conhecem apenas o contrato do motor.
- Fornecedor novo entra como projeto novo de provider + registro de engine; NUNCA como branch condicional dentro de provider existente.

## Contexto

Integrações de fornecedor mudam no ritmo do fornecedor. Sem uma fronteira anticorrupção, os tipos do parceiro se infiltram no domínio e cada mudança de payload externo vira refactor interno. Strategy + ACL isolam cada fornecedor num módulo substituível.

## Consequências

Adicionar fornecedor não toca domínio nem Application. Há tradução dupla (parceiro ↔ contrato do motor) em cada operação — custo deliberado que compra a estabilidade do núcleo.
