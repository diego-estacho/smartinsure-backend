---
id: ADR-005
title: Plataforma .NET 10 com C# 14 em toda a solution
status: accepted
tags: [plataforma]
applies-to: ["src/**"]
supersedes: []
evidence: []
---

# ADR-005: Plataforma .NET 10 com C# 14 em toda a solution

## Status

Aceito

## Decisão (normativa)

- Todo projeto da solution DEVE usar `<TargetFramework>net10.0</TargetFramework>`.
- Todo csproj DEVE habilitar `Nullable` e `ImplicitUsings`.
- Classes DEVEM usar primary constructors para injeção de dependência e file-scoped namespaces; block namespaces NUNCA são usados.
- Classes de UseCase, repositório e provider DEVEM ser `sealed` salvo necessidade explícita de herança.
- Bibliotecas DEVEM ser adotadas na versão estável mais recente compatível com .NET 10; versões preview NUNCA entram em produção.

## Contexto

A solution nasce em plataforma única e atual para maximizar suporte, performance e recursos de linguagem. Manter todos os projetos na mesma versão de framework e no mesmo dialeto de C# elimina divergência de estilo entre módulos e simplifica upgrade futuro em bloco.

## Consequências

Upgrades de framework acontecem em toda a solution de uma vez. O estilo único (nullable, primary constructors, file-scoped) é verificável em code review e por analisadores; código fora do dialeto é apontado como desvio.
