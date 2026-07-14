---
id: ADR-036
title: Repositório por agregado com DbContext como Unit of Work
status: accepted
tags: [dados]
applies-to: ["src/*.Infra.Data/Repositories/**"]
supersedes: []
evidence: []
---

# ADR-036: Repositório por agregado com DbContext como Unit of Work

## Status

Aceito

## Decisão (normativa)

- Cada agregado DEVE ter exatamente um repositório (`{Entidade}Repository : Repository<T>` implementando `I{Entidade}Repository` do Core).
- Repositórios "guarda-chuva" (métodos de vários agregados num só tipo) NUNCA são criados.
- O DbContext é o Unit of Work: `IUnitOfWork` encapsula o contexto e expõe `CommitAsync` e transação explícita; repositórios compartilham o mesmo contexto scoped do request.
- Repositórios DEVEM ser registrados uma única vez, via DI (Scoped); instanciação manual (`new`) de repositório NUNCA acontece.
- Repositório NUNCA chama `SaveChanges`; a conclusão da unidade de trabalho é responsabilidade do UseCase via `IUnitOfWork`.

## Contexto

O DbContext já implementa Unit of Work e Identity Map; recriar essa mecânica em camada própria duplica responsabilidade. Um repositório por agregado mantém as consultas coesas ao redor da raiz e evita a deriva para repositórios genéricos anêmicos ou catch-all.

## Consequências

Operações multi-agregado no mesmo request compõem naturalmente no mesmo commit. O UseCase controla o momento transacional; repositórios ficam livres de efeitos colaterais de persistência parcial.
