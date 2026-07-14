---
id: ADR-010
title: Grupo único api/v1 com autorização por default e versionamento por rota
status: accepted
tags: [api, seguranca]
applies-to: ["src/*.Api/**"]
supersedes: []
evidence: []
---

# ADR-010: Grupo único api/v1 com autorização por default e versionamento por rota

## Status

Aceito

## Decisão (normativa)

- Todos os Carter modules DEVEM ser montados sob um único grupo de rotas `api/v1` criado no `Program.cs`.
- O grupo DEVE aplicar `RequireAuthorization()` — toda rota nasce autenticada por default.
- Rota pública DEVE declarar `AllowAnonymous()` explicitamente na própria rota (opt-out visível em diff); rotas anônimas DEVEM ter autenticação alternativa quando forem webhooks (ver ADR-047).
- O versionamento de API é por segmento de rota (`api/v1`, futuramente `api/v2`); bibliotecas de versionamento NUNCA são adicionadas.

## Contexto

Autorização por default no grupo inverte o risco: esquecer configuração deixa a rota fechada, não aberta (fail-closed). Versionamento por rota é suficiente para uma API com um consumidor principal; bibliotecas de versionamento adicionam superfície de configuração sem necessidade atual.

## Consequências

Toda exposição pública é rastreável por grep de `AllowAnonymous`. Uma futura v2 implica novo grupo de rotas e coexistência explícita de versões, sem negociação por header.
