---
id: ADR-011
title: Pipeline de request único e centralizado
status: accepted
tags: [api]
applies-to: ["src/*.Api/Handlers/**", "src/*.Api/Endpoints/**"]
supersedes: []
evidence: []
---

# ADR-011: Pipeline de request único e centralizado

## Status

Aceito

## Decisão (normativa)

- Todo handler de endpoint DEVE delegar a execução ao pipeline central (`RequestHandler`): validação FluentValidation → execução do UseCase → mapeamento de exceções → resposta.
- try/catch dentro de handler de endpoint NUNCA é escrito; exceções são tratadas exclusivamente pelo resolver central.
- A conversão de exceção em ProblemDetails DEVE viver num único resolver (mapa da ADR-012); endpoints NUNCA constroem respostas de erro à mão.
- Toda dependência do pipeline DEVE ser resolvida via DI; instanciação manual de serviços (`new`) no pipeline é proibida.
- Handlers DEVEM receber o validator do request por DI e repassá-lo ao pipeline; validação NUNCA é invocada manualmente no corpo do handler.

## Contexto

Concentrar validação, execução e tratamento de erro num único ponto garante que toda resposta da API tem o mesmo formato e o mesmo comportamento de erro, independentemente de quem escreveu o endpoint. A alternativa (cada handler cuida do próprio try/catch) degrada em semânticas divergentes conforme o código cresce.

## Consequências

Casos genuinamente fora do fluxo padrão (download de arquivo, streaming) exigem extensão do pipeline central, nunca bypass local. O formato de resposta é uniforme e testável num único lugar.
