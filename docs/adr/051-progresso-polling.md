---
id: ADR-051
title: Acompanhamento de operação assíncrona por polling
status: accepted
tags: [assincrono, api]
applies-to: ["src/*.Api/**"]
supersedes: []
evidence: []
---

# ADR-051: Acompanhamento de operação assíncrona por polling

## Status

Aceito

## Decisão (normativa)

- O cliente acompanha operação assíncrona por polling em endpoint GET de status/resultado; o 202 da largada DEVE indicar onde consultar.
- O endpoint de status DEVE ser leitura barata do estado persistido (ADR-050), sem disparar processamento.
- Mecanismos de push (SSE, WebSocket, SignalR) NUNCA são implementados para progresso de operação.
- A resposta de status DEVE distinguir: em processamento, concluído (com resultado ou referência) e falhou (com ProblemDetails da ADR-012).

## Contexto

Push por SSE/WebSocket exige gerenciamento de conexões longas, buffers por cliente e cuidado com o gateway — infraestrutura desproporcional para o padrão de uso (operações de segundos consultadas por uma tela). Polling sobre estado persistido é stateless, compatível com qualquer gateway e trivial de testar.

## Consequências

A latência de percepção do cliente é o intervalo de polling — aceito para a natureza das operações. O endpoint de status precisa suportar a frequência de consulta das telas (leitura indexada barata).
