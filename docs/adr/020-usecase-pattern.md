---
id: ADR-020
title: UseCase pattern com interface por ação e estrutura de pasta padrão
status: accepted
tags: [application]
applies-to: ["src/*.Application.UseCase/UseCases/**"]
supersedes: []
evidence: []
---

# ADR-020: UseCase pattern com interface por ação e estrutura de pasta padrão

## Status

Aceito

## Decisão (normativa)

- Cada ação de negócio DEVE ser um UseCase: `sealed class {Ação}UseCase : I{Ação}UseCase`, com primary constructor para dependências.
- A interface da ação DEVE herdar do contrato base `IUseCase<TRequest, TResponse>`; ações sem retorno usam o tipo `Unit`.
- Cada UseCase DEVE viver em `UseCases/{Agregado}UseCases/{Ação}/` com subpastas `Interfaces/`, `Requests/`, `Responses/` e `Validators/`.
- Lógica de negócio de aplicação vive no UseCase; endpoints (ADR-009) e Functions (ADR-006) apenas delegam.
- Um UseCase DEVE ter uma única responsabilidade (uma ação); ações distintas NUNCA são combinadas por conveniência.

## Contexto

Um UseCase por ação dá unidade de teste, de autorização e de leitura — o nome do arquivo diz o que o sistema faz. Alternativas como application services largos por agregado concentram responsabilidades e degradam em "god classes" conforme o domínio cresce.

## Consequências

O número de arquivos cresce linearmente com as ações do sistema, compensado pela estrutura previsível (achável por convenção). O registro em DI é automatizado por scanning (ADR-021) para eliminar o custo de manutenção da lista.
