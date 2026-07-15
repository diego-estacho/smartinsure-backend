---
id: ADR-058
title: Nomes de artefatos em inglês; documentação e UI em pt-BR
status: accepted
tags: [dominio, api, plataforma]
applies-to: ["src/**", "tests/**", "docs/generated/**"]
supersedes: []
evidence: []
---

# ADR-058: Nomes de artefatos em inglês; documentação e UI em pt-BR

## Status

Aceito

## Decisão (normativa)

- Todo **artefato de código** DEVE ter nome em inglês: tipos, membros, arquivos, pastas, rotas de API, tabelas e colunas de banco, schemas do contrato OpenAPI e types gerados no front.
- Cada termo do glossário tem um **nome técnico em inglês mapeado 1:1** no próprio glossário (ex.: Oferta → `Offer`, Usuário → `User`). Nome técnico nasce no glossário junto com o termo — a tradução é por decreto, nunca ad hoc.
- **Documentação** (RNs, ADRs, exec-plans, READMEs), **UI** (labels, rotas de página, mensagens), mensagens de erro/validação da API e **commits** permanecem em pt-BR (ADR-019 segue valendo para cultura e mensagens).
- Status expostos pela API usam o nome técnico estável em inglês (ex.: `Pending`, `Active`); a UI traduz por mapa nome→label pt-BR (ADR de consumo do contrato no front).
- Nomes de métodos de teste seguem a convenção existente `Metodo_Resultado_Condicao` (ADR-056), com descrição em pt-BR permitida — não são renomeados por esta ADR.
- Esta ADR **supersede a regra estrutural do glossário** que exigia entidades de domínio em pt-BR no código. A regra anti-inversão de vocabulário passa a ser garantida pelo mapeamento 1:1 no glossário, não pela ausência de tradução.

## Contexto

O glossário nasceu com a regra "entidade em pt-BR no código" para eliminar inversão de vocabulário (ex.: trocar Oferta por Cotação na tradução). Na prática, o restante do código (utilitário, infra, testes, stack .NET) já é em inglês, e a mistura de idiomas dentro do mesmo artefato (`CriarUsuarioUseCase`, `ExisteEmailAsync`) cria um padrão híbrido pior de manter. Padrão único: artefato em inglês, negócio em pt-BR nos docs e na UI, com o glossário como tabela de tradução canônica.

## Consequências

- O glossário ganha a coluna de nome técnico; termo novo só entra com os dois nomes (aprovação da PO para o termo de negócio; arquitetura para o técnico).
- A inversão de vocabulário volta a ser um risco teórico — mitigada pelo mapeamento 1:1 versionado e pela revisão contra o glossário.
- Código de domínio existente foi renomeado no mesmo PR desta ADR (dois padrões nunca convivem — constituição, princípio 8).
- UI continua exibindo exclusivamente os termos pt-BR do glossário; rota de página no front permanece pt-BR (é interface com o corretor), rota de API é artefato técnico e fica em inglês.
