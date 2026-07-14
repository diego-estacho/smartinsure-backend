---
id: ADR-025
title: Composição entre UseCases proibida
status: accepted
tags: [application]
applies-to: ["src/*.Application.UseCase/**"]
supersedes: []
evidence: []
---

# ADR-025: Composição entre UseCases proibida

## Status

Aceito

## Decisão (normativa)

- Um UseCase NUNCA injeta ou invoca outro UseCase.
- Lógica compartilhada entre UseCases DEVE ser extraída para um serviço (de domínio ou de aplicação) injetado em ambos.
- Orquestração de múltiplas ações DEVE viver ou num UseCase próprio que usa serviços, ou no chamador (endpoint/Function invocando UseCases em sequência quando não há transação compartilhada).

## Contexto

UseCase chamando UseCase cria grafos de dependência invisíveis: autorização, validação e transação do UseCase interno executam em contexto inesperado, e o reuso "fácil" via DI (facilitado pelo scanning da ADR-021) degrada em acoplamento em cadeia. Serviço extraído torna o reuso explícito e sem a bagagem do pipeline do UseCase.

## Consequências

Reuso exige o passo deliberado de extrair serviço — atrito desejável que mantém o grafo raso. Cada UseCase permanece testável conhecendo apenas serviços e repositórios.
