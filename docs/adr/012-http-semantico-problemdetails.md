---
id: ADR-012
title: Códigos HTTP semânticos com mapa canônico de ProblemDetails
status: accepted
tags: [api]
applies-to: ["src/*.Api/**"]
supersedes: []
evidence: []
---

# ADR-012: Códigos HTTP semânticos com mapa canônico de ProblemDetails

## Status

Aceito

## Decisão (normativa)

- Códigos HTTP DEVEM seguir a semântica do protocolo; um código NUNCA expressa regra de negócio.
- Toda resposta de erro DEVE ser RFC 9457 (ProblemDetails) enriquecida com `traceId` e `correlationId`.
- O mapa canônico DEVE ser: 400 falha de validação de request (`ValidationProblemDetails`) ou request malformado; 401 não autenticado; 403 autenticado sem permissão; 404 recurso inexistente; 409 conflito de estado; 422 regra de negócio impede a operação; 500 erro inesperado; 503 dependência externa indisponível (a operação não pôde sequer ser tentada — ex.: provedor de identidade fora do ar, RN-005).
- Detalhe interno de exceção (stacktrace) NUNCA aparece na resposta em produção.
- Sucesso DEVE retornar o payload sem envelope; listagens DEVEM usar `PagedRequest`/`PagedResponse`.
- Payloads DEVEM ser serializados em camelCase (System.Text.Json).

## Contexto

Um mapa único de status permite que consumidores e agentes tratem erros programaticamente sem conhecer regras internas. A alternativa de colapsar todo erro de negócio em 400 esconde a diferença entre "não existe", "conflita" e "não pode", forçando o consumidor a interpretar mensagens de texto.

## Consequências

A camada Application precisa sinalizar a categoria do erro (ver ADR-022) para o resolver mapear o status. Recurso de outro tenant é invisível pelos query filters e resulta em 404, coerente com o mapa.
