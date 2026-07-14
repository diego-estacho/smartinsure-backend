---
id: ADR-055
title: Observabilidade via OpenTelemetry com correlação ponta a ponta
status: accepted
tags: [operacao]
applies-to: ["src/**"]
supersedes: []
evidence: []
---

# ADR-055: Observabilidade via OpenTelemetry com correlação ponta a ponta

## Status

Aceito

## Decisão (normativa)

- Traces, métricas e logs DEVEM ser emitidos via OpenTelemetry, exportando para Azure Monitor / Application Insights.
- O SDK clássico do Application Insights (`TelemetryClient` direto) NUNCA é usado em código de aplicação; instrumentação custom usa as APIs do OpenTelemetry (`ActivitySource`, `Meter`).
- Um `CorrelationId` DEVE ser propagado em toda chamada (recebido do gateway ou gerado na borda) e incluído em toda chamada de saída.
- Toda resposta de erro DEVE expor `traceId` e `correlationId` no ProblemDetails (ADR-012).
- Logs DEVEM passar por redação de dados sensíveis (Data Redaction): documento, credencial e dado pessoal NUNCA aparecem em log ou telemetria.
- Health checks DEVEM usar `Microsoft.Extensions.Diagnostics.HealthChecks`, expostos pela API em endpoint dedicado.

## Contexto

OpenTelemetry é o padrão aberto de instrumentação — mantém o destino (Azure Monitor) trocável e as bibliotecas instrumentadas automaticamente (HTTP, EF). A correlação ponta a ponta permite reconstruir um fluxo inteiro (request → usecase → parceiro) a partir do id devolvido ao cliente no erro.

## Consequências

Suporte recebe do usuário um id que localiza o trace exato. A redação de dados sensíveis é obrigatória e verificada em review de logging. A troca futura de backend de observabilidade não toca o código instrumentado.
