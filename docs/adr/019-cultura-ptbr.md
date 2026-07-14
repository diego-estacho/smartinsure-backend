---
id: ADR-019
title: Cultura fixa pt-BR no pipeline HTTP
status: accepted
tags: [api]
applies-to: ["src/*.Api/**"]
supersedes: []
evidence: []
---

# ADR-019: Cultura fixa pt-BR no pipeline HTTP

## Status

Aceito

## Decisão (normativa)

- O pipeline HTTP DEVE fixar a cultura `pt-BR` (request culture, formatação de data/número).
- Mensagens de erro de validação e de negócio DEVEM ser escritas em pt-BR.
- Negociação de idioma por header (`Accept-Language`) NUNCA é implementada.
- Código-fonte (tipos, membros, variáveis) permanece em inglês; pt-BR é o idioma das mensagens e do domínio exposto ao usuário.

## Contexto

O produto é mono-idioma por decisão de produto (mercado brasileiro). Fixar a cultura elimina bugs de parsing/formatação dependentes da cultura do servidor e dispensa infraestrutura de localização que não seria usada.

## Consequências

Internacionalização futura exigiria introduzir resource files e negociação de cultura — custo deliberadamente adiado. Formatação e mensagens são determinísticas em qualquer host.
