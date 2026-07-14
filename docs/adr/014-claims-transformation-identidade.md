---
id: ADR-014
title: Claims transformation como fonte única de identidade
status: accepted
tags: [api, seguranca]
applies-to: ["src/*.Api/**", "src/*.Infra.CrossCutting/Identity/**"]
supersedes: []
evidence: []
---

# ADR-014: Claims transformation como fonte única de identidade

## Status

Aceito

## Decisão (normativa)

- A identidade e o contexto do usuário (ids, perfil, tenant, roles) DEVEM vir exclusivamente das claims enriquecidas por uma implementação de `IClaimsTransformation`.
- O enriquecimento DEVE resolver os dados do usuário com cache por usuário para não consultar o banco a cada request.
- Código de endpoint/aplicação DEVE ler identidade via extensões de claims tipadas (ex.: `GetUserId()`, `GetBrokerId()`); parse manual do JWT cru do header NUNCA é feito fora do handler de autenticação.
- Claims derivadas do enriquecimento DEVEM ser as únicas usadas em policies de autorização (ver ADR-013).

## Contexto

Duas fontes de identidade (claims enriquecidas e re-parse do token) inevitavelmente divergem — cache, transformação e defaults se aplicam a uma e não à outra. Um único ponto de enriquecimento garante que toda a aplicação enxerga o mesmo usuário com o mesmo contexto.

## Consequências

O custo do enriquecimento é pago uma vez por request (amortizado por cache). Mudanças no shape do token são absorvidas num único ponto. A invalidação do cache de identidade precisa ser considerada quando o perfil do usuário muda.
