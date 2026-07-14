---
id: ADR-047
title: Webhooks inbound autenticados por header secreto com log de request
status: accepted
tags: [integracoes, seguranca]
applies-to: ["src/*.Api/Endpoints/**"]
supersedes: []
evidence: []
---

# ADR-047: Webhooks inbound autenticados por header secreto com log de request

## Status

Aceito

## Decisão (normativa)

- Endpoint de webhook DEVE ser `AllowAnonymous` (ADR-010) com validação obrigatória de header secreto compartilhado com o emissor, como primeira ação do handler.
- Header ausente ou inválido DEVE retornar 401 sem processar o payload.
- Todo request de webhook DEVE ser logado no MongoDB (headers relevantes + payload) antes do processamento, para diagnóstico e reprocessamento.
- O secret do header segue a ADR-054 (Options + variável de ambiente; nunca versionado).
- O processamento do webhook DEVE ser idempotente (ADR-050): reentrega do mesmo evento não duplica efeito.

## Contexto

Webhooks não carregam o JWT do usuário — a autenticação viável com os parceiros atuais é segredo compartilhado por header. O log integral do request compensa a natureza assíncrona: quando o processamento falha, o payload original está disponível para análise e reprocesso.

## Consequências

A segurança do canal depende da rotação e sigilo do secret por parceiro. O log em Mongo cria trilha completa de tudo que os parceiros enviaram, com o custo de armazenamento correspondente.
