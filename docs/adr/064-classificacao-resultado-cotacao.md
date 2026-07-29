---
id: ADR-064
title: Classificação do resultado da Cotação — status do parceiro traduzido para conjunto de domínio estável
status: accepted
tags: [dominio, integracoes]
applies-to: ["src/SmartInsure.Integration/CalculationEngines/**", "src/SmartInsure.Core/Enumerators/**", "src/SmartInsure.Core/Entities/Quotation.cs"]
supersedes: []
evidence: []
---

# ADR-064: Classificação do resultado da Cotação — status do parceiro traduzido para conjunto de domínio estável

## Status

Aceito em 2026-07-28 — ratificado por Diego Estácho no lugar da PO ([OPEN-07](../product-specs/open-decisions.md)); registrar confirmação da PO. Refina a etapa de cotações (RN-056..RN-063). Estende o ACL do Motor ([ADR-045](045-motor-services-providers-acl.md)) e a distinção Domain/Integration ([ADR-028](028-domain-integration-events.md)) para o resultado da Cotação; enums como string ([ADR-031](031-enums-string.md)). O conjunto de status do eixo imediato foi **conferido na fonte** (o gateway do fornecedor, que define os status).

## Contexto

Cada Cotação carrega o resultado que a Seguradora devolve pelo Motor de Cálculo (PLUG V2). Esse resultado chega no vocabulário do parceiro (sucesso, esteiras de análise, indisponibilidades, erros). Traduzir isso para o domínio é decisão difícil de reverter — e a experiência anterior expôs dois modos de falha a evitar:

1. **De-para espalhado** em vários pontos: um status novo do parceiro precisava ser mapeado em N lugares e, esquecido em um, caía num buraco (exceção na exibição ou classificação errada).
2. **Colapso silencioso do desconhecido**: um status novo/desconhecido convertido para uma classificação existente — chegando a exibir emissão automática e prêmio onde havia, na verdade, uma esteira de análise.

A resposta da cotação carrega o **status imediato** do resultado. O **status definitivo da proposta** (aprovada/recusada/cancelada) **não vem na resposta da cotação** — pertence ao acompanhamento da proposta (followup), fora desta fase.

## Decisão (normativa)

- O resultado da Cotação no domínio é um **conjunto pequeno e fechado** de classificações estáveis, persistidas como string (ADR-031): `Automatic`, `Analysis`, `Unavailable`, `Unrecognized`.
- **Motivo e esteira são dado que acompanha a classificação, não classificação nova.** A esteira da `Analysis` (`Underwriting`/`Credit`/`Pep`/`Reinsurance`/`Registration`, exposta por nome estável) e a lista de motivos do `Unavailable` são campos — assim um motivo novo do parceiro NÃO cria um status de domínio novo nem obriga tocar telas. O conjunto de esteiras é **completo**: o fornecedor sempre atribui uma esteira específica (a primeira regra que falha), então não existe "análise genérica" sem esteira.
- A tradução parceiro→domínio vive **num único lugar**: o mapper da ACL do PlugV2 (ADR-045). Nenhum `if` de status do parceiro fora da ACL; o modelo do parceiro nunca vaza para o domínio (ADR-028).
- Todo resultado que a ACL **não reconhece** DEVE recair em `Unrecognized` — **nunca** convertido em silêncio para outra classificação. `Unrecognized` é exibido sem prêmio, não é seguível, e é registrado/alertado para revisão (RN-058).
- Uma Cotação sem prêmio aplicável (`Analysis`, `Unavailable`, `Unrecognized`) NÃO expõe valor de prêmio.
- A **seguibilidade** (RN-059) é derivada de (classificação, esteira): `Automatic` e `Analysis`+`Underwriting` são seguíveis nesta fase; as demais não.
- **Contragarantia (CCG) é ortogonal à classificação, não uma esteira nem um status.** A resposta da cotação traz um veredito de que a Seguradora **exige CCG** para emitir, mais dados informativos (limite máximo sem CCG, se já assinada). Isso é capturado como **atributo da Cotação** e exibido ao corretor; uma Cotação `Automatic` pode exigir CCG. Uma Cotação que exige CCG **permanece seguível** — o corretor segue até a emissão normalmente e a exigência só é enforçada no emitir (barrado sem a CCG assinada — confirmado pela PO). O ciclo de assinatura do contrato de CCG é da **etapa de emissão** (fora desta fase).

## De-para PLUG V2 → resultado da Cotação (eixo imediato — 11 valores, conferidos na fonte)

> O conjunto abaixo é o **completo** do eixo imediato, conforme o gateway do fornecedor que define esses status. A decisão de negócio do **tomador nomeado** foi resolvida — indisponibilidade **informativa** (nomeação/transferência = evolução futura). Uma regra **conhecida** do gateway afeta o veredito por **cláusula particular** (`AllowAutomaticIssue`), fora deste eixo imediato e **não re-avaliada nesta fase** ([OPEN-17](../product-specs/open-decisions.md)).

| Resultado do parceiro (PLUG V2) | Classificação | Esteira / motivo |
|---|---|---|
| Sucesso / emissão automática | `Automatic` | — |
| Esteira de subscrição | `Analysis` | `Underwriting` (seguível — RN-059) |
| Esteira de cadastro | `Analysis` | `Registration` |
| Esteira de PEP | `Analysis` | `Pep` |
| Esteira de crédito | `Analysis` | `Credit` |
| Esteira de resseguro | `Analysis` | `Reinsurance` |
| Modalidade indisponível | `Unavailable` | motivo: modalidade indisponível |
| Cobertura indisponível | `Unavailable` | motivo: cobertura indisponível |
| Tomador nomeado | `Unavailable` | motivo: tomador nomeado — **indisponibilidade informativa** (nomeação/transferência = evolução futura) |
| Erro técnico / integração | `Unavailable` | motivo: falha técnica/integração (transitória — RN-057) |
| Desconhecido / não mapeado | `Unrecognized` | — |

**Ortogonal à tabela — Contragarantia (CCG):** a resposta traz o veredito booleano de **exigência de CCG** (+ limite máximo sem CCG, se já assinada), capturado como atributo da Cotação — não é linha desta tabela. A assinatura/contrato da CCG é da emissão (fora desta fase).

**Fora do eixo imediato — status da proposta (followup):** aprovada/recusada/cancelada só aparecem no acompanhamento da proposta após a cotação, não na resposta da cotação; entram na demanda de followup, não aqui.

**Motivo local (fora do provedor):** no modo *escolhidas* (RN-056), as Seguradoras habilitadas não selecionadas viram `Unavailable` com motivo **local** ("não incluída na solicitação") — motivo deliberado e conhecido nosso, não status do provedor mal-classificado; não fere a invariante (o `Unrecognized` continua reservado a status do provedor não mapeados).

**Cláusula particular × veredito (conhecido, PARKED — OPEN-17):** o gateway tem regra, documentada e replicada em ~11 plugins de Seguradora, em que uma cláusula particular `AllowAutomaticIssue=false` (não-fixa) encaminha a proposta para a esteira de subscrição em vez de emitir automaticamente. Nesta fase o Passo 4 **não re-avalia** o veredito por marcação de cláusula (captura a minuta — RN-062 — e mantém o resultado da cotação); Tags/texto da minuta **não** alteram o veredito. A decisão de re-avaliar aguarda a PO ([OPEN-17](../product-specs/open-decisions.md)) e, se confirmada, entra como regra (RN) sem reabrir esta ADR.

## Consequências

Suportar um motivo novo do parceiro é **dado** no mapper da ACL, não um status de domínio novo espalhado por telas — a classe de bug do "status novo em N lugares" some, e o desconhecido é sempre visível e seguro (nunca vira emissão/prêmio falso). Custo: a ACL exige teste cobrindo **cada** um dos 11 valores do eixo imediato, inclusive o caminho `Unrecognized`; a lista de motivos exibíveis cresce como dado (rótulos fora do domínio, ADR-031). A seguibilidade por (classificação, esteira) e a exigência de CCG são regra de negócio (RN-059, RN-058) — mudá-las é RN, não código solto. O status definitivo da proposta (recusa/cancelamento) e a assinatura da CCG entram na demanda de followup/emissão, sem reabrir esta ADR.
