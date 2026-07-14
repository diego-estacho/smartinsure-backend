---
id: ADR-016
title: CORS aberto com restrição de origem delegada ao gateway
status: accepted
tags: [seguranca, api]
applies-to: ["src/*.Api/**"]
supersedes: []
evidence: []
---

# ADR-016: CORS aberto com restrição de origem delegada ao gateway

## Status

Aceito

## Decisão (normativa)

- A API DEVE usar uma única política CORS aberta (qualquer origem/método/header), com os headers de resposta necessários expostos explicitamente.
- Restrição de origem é responsabilidade do gateway na frente da API; a aplicação NUNCA duplica essa regra.
- A API NUNCA é exposta publicamente sem o gateway na frente — esta é a premissa que sustenta a decisão; violá-la exige revisar esta ADR e a ADR-017 juntas.
- `AllowCredentials` NUNCA é combinado com origem aberta.

## Contexto

Com um gateway obrigatório na frente, duplicar a allowlist de origens na aplicação cria dois pontos de configuração que divergem. Concentrar a regra no gateway mantém a aplicação neutra a mudanças de frontend/domínio.

## Consequências

A segurança de origem depende integralmente da configuração do gateway. Ambientes locais funcionam sem fricção de CORS. Exposição acidental da API sem gateway deixa a política aberta — risco aceito e amarrado à premissa registrada.
