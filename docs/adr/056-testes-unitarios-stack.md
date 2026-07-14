---
id: ADR-056
title: Stack e convenções de teste unitário
status: accepted
tags: [testes]
applies-to: ["src/*.Tests/**"]
supersedes: []
evidence: []
---

# ADR-056: Stack e convenções de teste unitário

## Status

Aceito

## Decisão (normativa)

- A stack de teste unitário DEVE ser xUnit + NSubstitute + FluentAssertions + coverlet; Moq NUNCA é adicionado.
- A estrutura do projeto de testes DEVE espelhar `src` (ex.: teste de `UseCases/{Agregado}UseCases/{Ação}/` vive no mesmo path relativo).
- Nomes de teste DEVEM seguir `Metodo_Resultado_Condicao` (ex.: `ExecuteAsync_LancaNotFound_QuandoRecursoNaoExiste`).
- O SUT e seus substitutos DEVEM ser montados no construtor da classe de teste; factory methods privados cobrem variações.
- Fakes/stubs DEVEM ser usados apenas para fronteiras externas (repositórios, providers, mail, relógio); lógica interna do domínio NUNCA é substituída.
- Desenvolvimento DEVE seguir TDD (Red → Green → Refactor) para lógica de negócio.

## Contexto

Uma stack única e convenções de estrutura/nome tornam qualquer teste legível por qualquer membro do time e localizável por convenção. Substituir apenas fronteiras mantém os testes acoplados ao comportamento, não à implementação interna — refactor não quebra teste que não deveria quebrar.

## Consequências

Testes funcionam como documentação executável das ações do sistema. A disciplina de fronteira exige interfaces bem definidas nos pontos externos (já garantidas pelas ADRs 019, 040, 041, 044).
