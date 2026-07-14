---
id: ADR-007
title: Deploy por branch via GitHub Actions em Azure App Service
status: accepted
tags: [deploy-ci]
applies-to: [".github/workflows/**"]
supersedes: []
evidence: []
---

# ADR-007: Deploy por branch via GitHub Actions em Azure App Service

## Status

Aceito

## Decisão (normativa)

- O deploy DEVE ser disparado por push nas branches `develop`, `qa` e `master`, cada uma amarrada ao App Service do ambiente correspondente (dev, qa, produção).
- O publish DEVE ser `dotnet publish --configuration Release -r linux-x64 --self-contained`.
- A API e o app Functions DEVEM ser publicados como artefatos e deployáveis separados dentro do mesmo fluxo por branch.
- Deploy manual direto no App Service (fora do pipeline) NUNCA é feito em qa/produção.
- O workflow DEVE usar artifacts nomeados por branch/destino para rastreabilidade do que foi publicado.

## Contexto

O fluxo por branch dá um caminho previsível de promoção entre ambientes (develop → qa → master) com o Git como fonte da verdade do que está no ar. Alternativas como deploy por tag/release ou por aprovação manual adicionam cerimônia sem ganho para o tamanho atual do time.

## Consequências

O estado de cada ambiente é auditável pelo histórico da branch. Publish self-contained elimina dependência de runtime instalado no host, ao custo de artefatos maiores. Hotfix em produção exige passar pela branch `master`.
