---
id: ADR-027
title: Entidades ricas, sem Specification pattern
status: accepted
tags: [dominio]
applies-to: ["src/*.Core/Entities/**"]
supersedes: []
evidence: []
---

# ADR-027: Entidades ricas, sem Specification pattern

## Status

Aceito

## Decisão (normativa)

- Toda entidade DEVE ter setters privados; estado NUNCA é mutado por atribuição externa.
- Construção DEVE passar por construtor rico ou factory que estabelece o estado válido; construtor sem parâmetros fica `protected`/`private` (uso exclusivo do ORM).
- Transições de estado e invariantes DEVEM ser métodos de domínio na entidade (ex.: `Aprovar()`, `Cancelar(motivo)`), que validam e lançam as exceções tipadas da ADR-022 quando violados.
- Regras de consulta NUNCA usam Specification pattern; consultas vivem como métodos nos repositórios (ADR-023).
- Entidade anêmica (get/set público sem comportamento) NUNCA é aceita para agregados de negócio.

## Contexto

Setters públicos permitem que qualquer camada construa estados inválidos, empurrando as invariantes para validações espalhadas. Encapsular transições na entidade concentra as regras onde os dados vivem. Specifications foram descartadas: adicionam uma camada de indireção cujo benefício (composição de consultas) é coberto pelos métodos de repositório.

## Consequências

Escrever uma entidade exige desenhar seus métodos de transição — mais custo inicial, menos estados inválidos possíveis. O EF materializa via construtor protegido sem enfraquecer o encapsulamento público.
