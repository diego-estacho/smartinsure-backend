---
id: ADR-063
title: Classificação do resultado da Cotação — status do parceiro traduzido para conjunto de domínio estável
status: proposed
tags: [dominio, integracoes]
applies-to: ["src/SmartInsure.Integration/CalculationEngines/**", "src/SmartInsure.Core/Enumerators/**", "src/SmartInsure.Core/Entities/Quotation.cs"]
supersedes: []
evidence: []
---

# ADR-063: Classificação do resultado da Cotação — status do parceiro traduzido para conjunto de domínio estável

## Status

Proposto em 2026-07-27 — aguardando ratificação da PO ([OPEN-07](../product-specs/open-decisions.md)). Refina a etapa de cotações (RN-052..RN-057). Estende o ACL do Motor ([ADR-045](045-motor-services-providers-acl.md)) e a distinção Domain/Integration ([ADR-028](028-domain-integration-events.md)) para o resultado da Cotação; enums como string ([ADR-031](031-enums-string.md)).

## Contexto

Cada Cotação carrega o resultado que a Seguradora devolve pelo Motor de Cálculo (PLUG V2). Esse resultado chega no vocabulário do parceiro (sucesso, esteiras de análise, indisponibilidades, recusas, erros de integração). Traduzir isso para o domínio é decisão difícil de reverter — e a experiência anterior expôs dois modos de falha a evitar:

1. **De-para espalhado** em vários pontos: um status novo do parceiro precisava ser mapeado em N lugares e, esquecido em um, caía num buraco (exceção na exibição ou classificação errada).
2. **Colapso silencioso do desconhecido**: um status novo/desconhecido convertido para uma classificação existente — chegando a exibir emissão automática e prêmio onde havia, na verdade, uma esteira de análise.

## Decisão (normativa)

- O resultado da Cotação no domínio é um **conjunto pequeno e fechado** de classificações estáveis, persistidas como string (ADR-031): `Automatic`, `Analysis`, `Unavailable`, `Unrecognized`.
- **Motivo e esteira são dado que acompanha a classificação, não classificação nova.** A esteira da `Analysis` (`Underwriting`/`Credit`/`Pep`/`Reinsurance`/`Registration`, exposta por nome estável) e a lista de motivos do `Unavailable` são campos — assim uma esteira ou um motivo novo do parceiro NÃO cria um status de domínio novo nem obriga tocar telas.
- A tradução parceiro→domínio vive **num único lugar**: o mapper da ACL do PlugV2 (ADR-045). Nenhum `if` de status do parceiro fora da ACL; o modelo do parceiro nunca vaza para o domínio (ADR-028).
- Todo resultado que a ACL **não reconhece** DEVE recair em `Unrecognized` — **nunca** convertido em silêncio para outra classificação. `Unrecognized` é exibido sem prêmio, não é seguível, e é registrado/alertado para revisão (RN-054).
- Uma Cotação sem prêmio aplicável (`Analysis`, `Unavailable`, `Unrecognized`) NÃO expõe valor de prêmio.
- A **seguibilidade** (RN-055) é derivada de (classificação, esteira): `Automatic` e `Analysis`+`Underwriting` são seguíveis nesta fase; as demais não.

## De-para PLUG V2 → resultado da Cotação (referência — a confirmar contra o contrato vigente)

> Esta tabela é a **compreensão atual** dos resultados possíveis do PLUG V2 e serve como referência para o mapper da ACL. Os códigos concretos e os casos de julgamento marcados **[A CONFIRMAR]** DEVEM ser validados contra o contrato vigente do PLUG V2 e ratificados pela PO antes do código.

| Resultado do parceiro (PLUG V2) | Classificação | Esteira / motivo |
|---|---|---|
| Sucesso / emissão automática | `Automatic` | — |
| Esteira de subscrição | `Analysis` | `Underwriting` (seguível — RN-055) |
| Esteira de cadastro | `Analysis` | `Registration` |
| Esteira de PEP | `Analysis` | `Pep` |
| Esteira de crédito | `Analysis` | `Credit` |
| Esteira de resseguro | `Analysis` | `Reinsurance` |
| Modalidade indisponível | `Unavailable` | motivo: modalidade indisponível |
| Cobertura indisponível | `Unavailable` | motivo: cobertura indisponível |
| Tomador nomeado | `Unavailable` | motivo: tomador nomeado — **[A CONFIRMAR: caso à parte/acionável?]** |
| Recusa da Seguradora | `Unavailable` | motivos informados pela Seguradora |
| Falha de integração (timeout/erro do parceiro) | `Unavailable` | motivo: falha de integração (transitória, RN-053) — **[A CONFIRMAR: distinguir de recusa de negócio p/ permitir re-tentar]** |
| Desconhecido / novo / não mapeado | `Unrecognized` | — |

## Consequências

Suportar uma esteira ou motivo novo do parceiro é **dado** no mapper da ACL, não um status de domínio novo espalhado por telas — a classe de bug do "status novo em N lugares" some, e o desconhecido é sempre visível e seguro (nunca vira emissão/prêmio falso). Custo: a ACL exige teste cobrindo **cada** resultado do parceiro, inclusive o caminho `Unrecognized`; a lista de esteiras/motivos exibíveis cresce como dado (rótulos fora do domínio, ADR-031). A seguibilidade por (classificação, esteira) é regra de negócio (RN-055) — mudá-la é RN, não código solto. Se um dia a granularidade fina de resultado precisar virar comportamento (ex.: tratar cada recusa diferente), entra por dado/esteira, sem reabrir esta ADR.
