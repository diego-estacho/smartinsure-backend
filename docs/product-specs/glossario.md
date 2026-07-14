# Glossário canônico do domínio

Status: **proposta** — aguardando ratificação da PO ([OPEN-01](open-decisions.md)).

Este arquivo é o item nº 1 da fonte de verdade do harness. Nenhum nome de entidade, tabela, rota, tela, evento ou status nasce fora daqui.

**Regra estrutural:** no código, as entidades de domínio usam exatamente estes termos em pt-BR (`Oferta`, `Cotacao`, `Proposta`, `Apolice`) — sem tradução para inglês no domínio. A tradução de termo de negócio é onde nasce inversão de vocabulário; eliminando a tradução, eliminamos a classe inteira do bug.

## Termos

| Termo | Definição | Cardinalidade | O que NUNCA chamar assim |
|---|---|---|---|
| **Oferta** | O pedido/estudo que o corretor cria no wizard (tomador, modalidade, valores, vigência) | 1 por jornada | o retorno de uma seguradora |
| **Cotação** | O retorno de UMA seguradora para uma oferta: prêmio, condições, prazo | N por oferta (uma por seguradora) | o pedido do corretor |
| **Proposta** | A cotação aceita pelo corretor, em processamento na seguradora até a emissão | 0..1 por oferta | qualquer coisa antes do aceite |
| **Apólice** | O documento emitido pela seguradora | 0..1 por proposta | — |
| **Seguradora** | Quem precifica e emite. A OnPoint é um *hub* de seguradoras, não uma seguradora | — | — |
| **Corretora / Corretor** | A empresa cliente da plataforma / o usuário dela | — | — |

Origem: ontologia definida pelo negócio em 2026-05-22 ("Oferta (singular) → Cotações, uma por seguradora"). Se a PO decidir termos diferentes, este arquivo muda ANTES de qualquer código de domínio existir.

## Status

A máquina de estados do Smart será enumerada nesta seção junto com a PO, antes do primeiro código de domínio ([OPEN-01](open-decisions.md)). Regras já fixadas:

- Status é exposto na API **por nome estável**, nunca por número ordinal.
- Status de seguradora não aparece na UI nem no domínio — somente a tradução dele, feita na integração daquela seguradora.
- Cada status novo exige: entrada aqui, transições permitidas documentadas, e teste de transição no módulo dono.
