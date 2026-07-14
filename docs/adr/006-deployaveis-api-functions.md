---
id: ADR-006
title: Dois deployáveis — API HTTP e app Azure Functions para tarefas agendadas
status: accepted
tags: [plataforma, deploy-ci, assincrono]
applies-to: ["src/*.Api/**", "src/*.Functions/**"]
supersedes: []
evidence: []
---

# ADR-006: Dois deployáveis — API HTTP e app Azure Functions para tarefas agendadas

## Status

Aceito

## Decisão (normativa)

- A solution DEVE ter exatamente dois deployáveis: a API HTTP e um app Azure Functions.
- Tarefas agendadas/recorrentes DEVEM viver no app Azure Functions (modelo isolated worker) como Timer Triggers; a API NUNCA hospeda jobs ou schedulers.
- Cada Function DEVE apenas orquestrar: delega a services/usecases da camada Application; lógica de negócio NUNCA vive no corpo da Function.
- Toda execução de tarefa agendada DEVE registrar auditoria (início, fim, erros) no MongoDB.
- Timer Triggers DEVEM ser configurados para não sobrepor execuções da mesma tarefa.

## Contexto

Trabalho agendado tem ciclo de vida, escala e janela de deploy distintos da API. Hospedar jobs dentro do processo da API acopla reinícios e deploys ao processamento em andamento. Um scheduler embutido em Worker Service próprio foi descartado em favor do agendamento gerenciado das Azure Functions (Timer Trigger nativo, retry e monitoramento da plataforma).

## Consequências

Deploy da API não interrompe tarefas agendadas e vice-versa. O time mantém dois pipelines de publicação (ver ADR-007). A auditoria em Mongo dá visibilidade de execução sem depender só da telemetria da plataforma.
