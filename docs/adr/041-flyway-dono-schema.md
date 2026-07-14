---
id: ADR-041
title: Flyway como único dono do schema, em repositório dedicado
status: accepted
tags: [migrations, dados]
applies-to: ["src/*.Infra.Data/**", "migrations/**"]
supersedes: []
evidence: []
---

# ADR-041: Flyway como único dono do schema, em repositório dedicado

## Status

Aceito

## Decisão (normativa)

- Toda mudança de schema DEVE ser uma migration Flyway no repositório dedicado de migrations.
- EF Migrations (`dotnet ef migrations`) NUNCA são usadas; o projeto de dados NUNCA contém pasta `Migrations/` do EF.
- As migrations DEVEM ser aplicadas exclusivamente pelo CI, no fluxo de PR develop → qa → master (uma branch por ambiente); aplicação manual em qa/produção é proibida.
- Scripts de migration NUNCA são copiados para outros repositórios; o repo de migrations é a fonte única.
- Mudança de modelo EF (mapping) e a migration correspondente DEVEM andar juntas na mesma janela de release; deploy de código que depende de schema ainda não migrado é proibido.

## Contexto

Duas ferramentas de schema (EF Migrations + Flyway) criam duas verdades sobre o banco. Um repositório dedicado com aplicação via CI dá trilha de auditoria por ambiente e desacopla o ciclo do schema do ciclo do código, mantendo a ordem: banco migra antes do código que o consome.

## Consequências

O desenvolvedor escreve SQL explícito em vez de gerar diff automático — mais controle, mais responsabilidade. A coordenação código×schema entre dois repositórios é um ponto de atenção de release, mitigado pela regra de ordem de deploy.
