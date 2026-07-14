---
id: ADR-053
title: Options Pattern para toda configuração externa
status: accepted
tags: [operacao, plataforma]
applies-to: ["src/**"]
supersedes: []
evidence: []
---

# ADR-053: Options Pattern para toda configuração externa

## Status

Aceito

## Decisão (normativa)

- Toda configuração externa DEVE ser lida via Options Pattern: uma classe Options por seção do appsettings, com correspondência 1:1 entre classe e seção.
- O registro DEVE ser `AddOptions<T>().BindConfiguration("Secao")` com validação e `ValidateOnStart()` — configuração inválida ou ausente falha o startup.
- `IOptions<T>` é o padrão de consumo; `IOptionsSnapshot<T>`/`IOptionsMonitor<T>` só com necessidade explícita de reload documentada.
- Cada integração DEVE ter sua própria classe Options; classes de configuração compartilhadas entre integrações NUNCA existem.
- Classes estáticas NUNCA leem configuração; ficam restritas a constantes da aplicação (Roles, Policies, CacheKeys, Routes).
- `IConfiguration` NUNCA é injetada em código de negócio; só no composition root.

## Contexto

Configuração em classes estáticas mutáveis cria estado global preenchido no startup: invisível para o container, impossível de substituir em teste e propenso a leitura antes da escrita. Options tipadas com `ValidateOnStart` movem o erro de configuração do runtime (request falhando) para o deploy (startup falhando).

## Consequências

Toda configuração tem dono, shape e validação declarados. Ambientes mal configurados não sobem — o erro aparece no deploy, com mensagem apontando a seção. Testes substituem Options por instância direta, sem fixtures de configuração.
