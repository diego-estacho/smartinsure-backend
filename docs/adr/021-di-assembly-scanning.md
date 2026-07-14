---
id: ADR-021
title: Registro de DI por assembly scanning
status: accepted
tags: [application, plataforma]
applies-to: ["src/*.Application.UseCase/IoC/**", "src/*.Api/Extensions/**"]
supersedes: []
evidence: []
---

# ADR-021: Registro de DI por assembly scanning

## Status

Aceito

## Decisão (normativa)

- UseCases DEVEM ser registrados por assembly scanning na convenção `I{Ação}UseCase` → `{Ação}UseCase`, lifetime Scoped.
- Validators DEVEM ser registrados via `AddValidatorsFromAssembly`.
- Registro manual um-a-um de UseCases/validators NUNCA é feito; um tipo que segue a convenção é registrado automaticamente.
- Serviços fora da convenção (decorators, múltiplas implementações da mesma interface) DEVEM ser registrados explicitamente ao lado do scanning, com comentário do porquê.
- A convenção de scanning DEVE ser coberta por teste (tipo novo que segue a convenção resolve pelo container).

## Contexto

Registro manual cresce linearmente com o número de UseCases e falha silenciosamente por esquecimento (erro só em runtime). Scanning por convenção elimina essa classe de erro e mantém o composition root enxuto. A explicitude perdida é recuperada pela convenção única e testada.

## Consequências

O container fica dependente da convenção de nomes — desvios de naming quebram resolução, o que é desejável (força a convenção). Registro explícito passa a ser sinal de exceção deliberada, visível em review.
