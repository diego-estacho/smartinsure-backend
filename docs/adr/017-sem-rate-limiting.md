---
id: ADR-017
title: Sem rate limiting na aplicação
status: accepted
tags: [seguranca, api]
applies-to: ["src/*.Api/**"]
supersedes: []
evidence: []
---

# ADR-017: Sem rate limiting na aplicação

## Status

Aceito

## Decisão (normativa)

- A API NUNCA implementa rate limiting em código de aplicação (middleware, atributo ou biblioteca).
- Proteção contra abuso/volume é responsabilidade do gateway/WAF na frente da API.
- Premissa (compartilhada com a ADR-016): a API nunca é exposta sem o gateway; se a premissa cair, esta ADR é revisada.

## Contexto

Rate limiting na aplicação e no gateway ao mesmo tempo cria duas políticas para manter coerentes. Como o gateway é obrigatório e é o ponto natural de controle de tráfego, a aplicação permanece sem essa responsabilidade.

## Consequências

A aplicação não tem defesa própria contra rajadas se o gateway falhar em aplicá-la. Endpoints custosos devem considerar limites de negócio (paginação, tamanho de payload) — que não são rate limiting e continuam valendo.
