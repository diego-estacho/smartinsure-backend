---
id: ADR-028
title: Domain Events e Integration Events com papéis distintos
status: accepted
tags: [dominio, integracoes]
applies-to: ["src/*.Core/Events/**", "src/*.Application.UseCase/Events/**"]
supersedes: []
evidence: []
---

# ADR-028: Domain Events e Integration Events com papéis distintos

## Status

Aceito

## Decisão (normativa)

- Domain Events DEVEM viver no Core (`Core/Events/`) e representar fatos relevantes do negócio, nomeados no passado (ex.: `PropostaAprovada`).
- Domain Events DEVEM ser disparados apenas após alterações válidas do domínio; NUNCA carregam contratos, DTOs ou tipos de sistemas externos.
- Handlers de Domain Events DEVEM viver na camada Application.
- Integration Events são contratos de comunicação com sistemas externos: DEVEM ser definidos e publicados fora do Core (Application/Providers) e NUNCA são reusados como Domain Events (nem o inverso).
- A tradução Domain Event → Integration Event, quando existir, DEVE ser explícita num handler da Application.

## Contexto

Misturar os dois tipos de evento acopla o domínio a contratos externos: uma mudança de payload de parceiro passaria a quebrar o domínio. Separar fato interno (domínio) de contrato externo (integração) permite evoluir cada um no seu ritmo.

## Consequências

Comunicação com o mundo externo sempre tem um ponto de tradução explícito. Há duplicação deliberada de shape entre evento interno e contrato externo quando os dois existem para o mesmo fato.
