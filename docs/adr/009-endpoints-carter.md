---
id: ADR-009
title: Endpoints exclusivamente via Carter modules com convenção por entidade
status: accepted
tags: [api]
applies-to: ["src/*.Api/Endpoints/**"]
supersedes: []
evidence: []
---

# ADR-009: Endpoints exclusivamente via Carter modules com convenção por entidade

## Status

Aceito

## Decisão (normativa)

- Todo endpoint HTTP DEVE ser definido num Carter module: `{Entidade}Endpoint : CarterModule`, um arquivo por módulo em `Endpoints/`.
- MVC Controllers (`[ApiController]`, `ControllerBase`) NUNCA são usados.
- Cada módulo DEVE declarar seu prefixo de rota no construtor (`base("entidades")`) e agrupar todas as rotas daquela entidade; dois módulos NUNCA compartilham o mesmo prefixo.
- Handlers DEVEM ser métodos `private static` dentro do módulo.
- Cada rota DEVE declarar metadados OpenAPI (`WithName`, `WithSummary`, `Produces<T>`).
- Módulos avulsos de rota única NUNCA são criados; a rota entra no módulo da entidade correspondente.

## Contexto

Minimal APIs com Carter dão roteamento explícito e módulos pequenos sem a cerimônia de Controllers MVC (filters, model binding implícito, herança). A convenção um-módulo-por-entidade torna trivial localizar onde uma rota vive e evita a fragmentação de rotas da mesma entidade em arquivos diferentes.

## Consequências

O roteamento fica plano e legível, sem pipeline MVC. Recursos de MVC (filters globais, conventions) não estão disponíveis — middlewares e o pipeline central de request (ADR-011) cumprem esse papel.
