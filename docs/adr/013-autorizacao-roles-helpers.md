---
id: ADR-013
title: Autorização por role via helpers de extensão e policies nomeadas fail-closed
status: accepted
tags: [api, seguranca]
applies-to: ["src/*.Api/Endpoints/**", "src/*.Api/Extensions/**"]
supersedes: []
evidence: []
---

# ADR-013: Autorização por role via helpers de extensão e policies nomeadas fail-closed

## Status

Aceito

## Decisão (normativa)

- Exigência de role em rota DEVE usar exclusivamente os helpers de extensão (`.RequireRoles(...)` e helpers de policy nomeada).
- `AuthorizeAttribute` inline em rota NUNCA é usado.
- Policies nomeadas DEVEM ser fail-closed: exigem a presença de claim que só existe após o enriquecimento de identidade (ADR-014); um token válido porém não enriquecido NUNCA satisfaz uma policy.
- Nomes de role e de policy DEVEM ser constantes centralizadas (ex.: `UserRoles`, `AuthPolicies`); strings literais de role NUNCA aparecem em endpoint.
- Novas combinações de exigência DEVEM virar helper/policy nomeada, não composição ad-hoc na rota.

## Contexto

Um único mecanismo torna a superfície de autorização auditável por grep e uniforme em code review. Policies fail-closed impedem que um principal "cru" (token válido sem enriquecimento) passe por omissão de configuração — o erro de configuração falha fechado.

## Consequências

Toda evolução de autorização passa pelos helpers centralizados, criando um catálogo único de exigências. O custo é criar um helper novo quando surge combinação inédita, em troca de nunca ter regra de acesso escondida em atributo local.
