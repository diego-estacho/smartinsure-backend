---
id: ADR-057
title: Política de cobertura e escopo da suíte de testes
status: accepted
tags: [testes]
applies-to: ["src/*.Tests/**"]
supersedes: []
evidence: []
---

# ADR-057: Política de cobertura e escopo da suíte de testes

## Status

Aceito

## Decisão (normativa)

- A cobertura DEVE priorizar casos de uso críticos: UseCases, validators e comportamento de domínio (entidades ricas) têm teste obrigatório.
- Repositórios NUNCA têm teste unitário dedicado (mocks de repositório nos testes de UseCase são a fronteira).
- Massas de cenários canônicos DEVEM usar testes data-driven (golden scenarios via `[Theory]` + dados versionados).
- Testes de integração estão fora do escopo inicial do projeto; quando adotados, o caminho DEVE ser WebApplicationFactory in-process com fronteiras substituídas em `ConfigureTestServices`.
- Risco registrado e aceito: enquanto não houver testes de integração, mapeamentos EF, query filters e constraints não têm verificação automatizada contra banco real — mudanças nessas áreas DEVEM receber verificação manual dirigida no review.
- Métrica de cobertura (coverlet) é informativa; percentual NUNCA é gate cego de PR.

## Contexto

O valor da suíte concentra-se onde vive a lógica: casos de uso e domínio. Testar repositório com banco falso verifica pouco; testar com banco real é infraestrutura de integração — deliberadamente adiada para o início do projeto, com o risco explícito em vez de coberto por uma suíte de baixa fidelidade.

## Consequências

A suíte inicial é rápida e sem dependência de infraestrutura. A dívida de verificação de persistência é conhecida, tem dono (review dirigido) e caminho de resolução definido (adoção futura de integração in-process).
