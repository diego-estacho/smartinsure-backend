---
id: ADR-023
title: Acesso a dados exclusivamente via UnitOfWork e repositórios
status: accepted
tags: [application, dados]
applies-to: ["src/*.Application.UseCase/**"]
supersedes: []
evidence: []
---

# ADR-023: Acesso a dados exclusivamente via UnitOfWork e repositórios

## Status

Aceito

## Decisão (normativa)

- UseCases DEVEM acessar dados exclusivamente por `IUnitOfWork` e pelas interfaces de repositório definidas no Core.
- O DbContext NUNCA é injetado ou referenciado na camada Application; o projeto Application NUNCA referencia o projeto de infraestrutura de dados.
- Consultas específicas de uma ação DEVEM virar método no repositório do agregado correspondente, retornando entidade ou read-model (ADR-032).
- Persistência DEVE ser concluída por `IUnitOfWork.CommitAsync`; transação explícita multi-agregado usa `BeginTransaction/Commit/Rollback` do UnitOfWork.
- A regra é travada por teste de arquitetura (ADR-052); o teste NUNCA é desligado.

## Contexto

Com o DbContext acessível na Application, cada UseCase vira um ponto de acoplamento ao ORM e as consultas se espalham sem contrato. Repositórios concentram as consultas por agregado, dão superfície de substituição em teste unitário e mantêm a direção de dependência da Clean Architecture (Application → contratos no Core ← Infra).

## Consequências

Repositórios crescem com métodos de consulta específicos — aceito em troca de contrato explícito e testabilidade. Toda mudança de acesso a dados fica confinada à camada de infraestrutura.
