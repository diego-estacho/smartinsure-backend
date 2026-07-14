---
id: ADR-054
title: Secrets fora do repositório
status: accepted
tags: [operacao, seguranca]
applies-to: ["src/**", ".github/workflows/**"]
supersedes: []
evidence: []
---

# ADR-054: Secrets fora do repositório

## Status

Aceito

## Decisão (normativa)

- Nenhum secret (senha, connection string com credencial, API key, signing key) é versionado em NENHUM arquivo do repositório, sem exceção.
- `appsettings.{Ambiente}.json` versionados DEVEM conter os campos sensíveis vazios ou ausentes.
- Desenvolvimento local DEVE usar `appsettings.{Ambiente}.Local.json` (gitignored) para secrets.
- Ambientes (dev/qa/produção) DEVEM receber secrets por variável de ambiente do host (App Service/Functions configuration), consumidos pelas Options da ADR-053.
- Pipelines DEVEM usar o secret store do GitHub Actions; secret NUNCA aparece em workflow ou log.
- Secret exposto acidentalmente DEVE ser rotacionado imediatamente — remover do histórico não basta.

## Contexto

Secret em repositório tem vida útil ilimitada: clones, forks e histórico preservam o valor mesmo após remoção. A combinação Local.json (dev) + variáveis de ambiente (hosts) cobre todos os cenários sem nenhum valor sensível em arquivo versionado.

## Consequências

Setup de dev exige criar o Local.json a partir de template documentado. Rotação de secret é operação de configuração de host, sem deploy de código. Vazamento por repositório deixa de ser um vetor.
