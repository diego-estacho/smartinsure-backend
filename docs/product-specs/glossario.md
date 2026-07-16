# Glossário canônico do domínio

Status: **proposta** — aguardando ratificação da PO ([OPEN-01](open-decisions.md)).

Este arquivo é o item nº 1 da fonte de verdade do harness. Nenhum nome de entidade, tabela, rota, tela, evento ou status nasce fora daqui.

**Regra estrutural (ADR-058):** todo artefato de código usa o **nome técnico em inglês** mapeado 1:1 nesta tabela; UI, documentação e mensagens usam exclusivamente o termo pt-BR. A tradução é por decreto — só existe aqui; nome técnico ad hoc é inversão de vocabulário e reprova em review.

## Termos

| Termo | Nome técnico (código) | Definição | Cardinalidade | O que NUNCA chamar assim |
|---|---|---|---|---|
| **Oferta** | `Offer` | O pedido/estudo que o corretor cria no wizard (tomador, modalidade, valores, vigência) | 1 por jornada | o retorno de uma seguradora |
| **Cotação** | `Quote` | O retorno de UMA seguradora para uma oferta: prêmio, condições, prazo | N por oferta (uma por seguradora) | o pedido do corretor |
| **Proposta** | `Proposal` | A cotação aceita pelo corretor, em processamento na seguradora até a emissão | 0..1 por oferta | qualquer coisa antes do aceite |
| **Apólice** | `Policy` | O documento emitido pela seguradora | 0..1 por proposta | — |
| **Seguradora** | `Insurer` | Quem precifica e emite. A OnPoint é um *hub* de seguradoras, não uma seguradora | — | — |
| **Corretora / Corretor** | `Brokerage` / `Broker` | A empresa cliente da plataforma / o usuário dela | — | — |
| **Usuário** | `User` | Pessoa que acessa a plataforma, com identidade mantida no provedor de identidade (ratificado pela PO em 2026-07-15) | — | a Corretora (empresa) |
| **Provedor de identidade** | `IdentityProvider` | Serviço externo que guarda credenciais e autentica os Usuários da plataforma (ratificado pela PO em 2026-07-15) | — | — |
| **Birô** | `Bureau` | Serviço externo que fornece dados cadastrais públicos de pessoa ou empresa a partir do CPF/CNPJ (ratificado pela PO em 2026-07-15) | — | fonte interna de dados; a seguradora |
| **Perfil** | `Profile` | Papel atribuído a um Usuário que autoriza operações restritas da plataforma; Usuário sem Perfil é usuário comum (proposto em 2026-07-16 — aguardando ratificação da PO) | 0..1 por Usuário nesta fase | cargo na Corretora |
| **Administrador do Sistema** | `SystemAdministrator` | Perfil da equipe SmartInsure que autoriza operações internas da plataforma, como manter o catálogo de Seguradoras (proposto em 2026-07-16 — aguardando ratificação da PO) | — | usuário de Corretora |

Origem: ontologia definida pelo negócio em 2026-05-22 ("Oferta (singular) → Cotações, uma por seguradora"). Se a PO decidir termos diferentes, este arquivo muda ANTES de qualquer código de domínio existir.

## Status

A máquina de estados do Smart será enumerada nesta seção junto com a PO, antes do primeiro código de domínio ([OPEN-01](open-decisions.md)). Regras já fixadas:

- Status é exposto na API **por nome estável**, nunca por número ordinal.
- Status de seguradora não aparece na UI nem no domínio — somente a tradução dele, feita na integração daquela seguradora.
- Cada status novo exige: entrada aqui, transições permitidas documentadas, e teste de transição no módulo dono.

### Usuário (ratificado pela PO em 2026-07-15)

| Status | Nome estável (API) | Significado | Transições permitidas |
|---|---|---|---|
| **Pendente** | `Pending` | Usuário criado que ainda não concluiu o primeiro acesso | Pendente → Ativo (RN-002) |
| **Ativo** | `Active` | Usuário que concluiu o primeiro acesso com senha própria definida | — (inativação ainda não definida) |

### Seguradora (proposto em 2026-07-16 — aguardando ratificação da PO)

| Status | Nome estável (API) | Significado | Transições permitidas |
|---|---|---|---|
| **Ativa** | `Active` | Seguradora em operação no catálogo, visível aos fluxos operacionais | Ativa → Inativa (RN-007) |
| **Inativa** | `Inactive` | Seguradora fora de operação — permanece no catálogo, fora da visão operacional (nunca excluída) | Inativa → Ativa (RN-007) |
