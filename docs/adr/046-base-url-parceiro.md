---
id: ADR-046
title: Resolução de base URL de parceiro com hierarquia única
status: accepted
tags: [integracoes, operacao]
applies-to: ["src/*.Providers.*/**"]
supersedes: []
evidence: []
---

# ADR-046: Resolução de base URL de parceiro com hierarquia única

## Status

Aceito

## Decisão (normativa)

- A base URL de parceiro DEVE resolver por uma única hierarquia: configuração da engine (banco) como fonte primária → override por variável de ambiente (para ambientes de teste).
- Default hardcoded de URL NUNCA existe em código.
- Ausência de configuração DEVE falhar no startup ou na resolução da engine com erro explícito — nunca silenciosamente cair num endereço embutido.
- A hierarquia DEVE estar implementada num único ponto (factory do provider); consumidores NUNCA resolvem URL por conta própria.

## Contexto

Múltiplas fontes para o mesmo endereço (config, env var, constante, appsettings) tornam impossível responder "pra onde isso aponta agora" sem ler todo o código. Uma hierarquia única com falha explícita elimina a classe de bug "ambiente de teste chamando produção por fallback".

## Consequências

Configuração faltante aparece imediatamente como erro de startup/resolução, não como chamada ao destino errado. Ambientes de teste trocam o destino por env var sem tocar dados da engine.
