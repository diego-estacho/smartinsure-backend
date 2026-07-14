---
id: ADR-008
title: Build e testes como gate obrigatório de PR
status: accepted
tags: [deploy-ci, testes]
applies-to: [".github/workflows/**"]
supersedes: []
evidence: []
---

# ADR-008: Build e testes como gate obrigatório de PR

## Status

Aceito

## Decisão (normativa)

- Todo PR DEVE rodar `dotnet build` e `dotnet test` da solution completa no CI.
- O merge NUNCA acontece com build ou testes falhando; o gate é bloqueante, não informativo.
- Testes NUNCA são desabilitados/skipados para destravar um merge; teste quebrado é corrigido ou o código é ajustado.
- O pipeline de PR DEVE compilar todos os deployáveis (API e Functions), não apenas o projeto alterado.

## Contexto

O gate de PR é o único ponto que garante que a suíte de testes reflete o estado real do código antes da integração. Um CI que apenas compila permite regressão silenciosa: o custo de descobrir teste quebrado migra do autor do PR para quem faz o próximo deploy.

## Consequências

PRs ficam mais lentos na proporção do tamanho da suíte — investimento aceito em troca de regressão detectada na origem. A saúde da suíte vira responsabilidade de todo PR, não de uma limpeza periódica.
