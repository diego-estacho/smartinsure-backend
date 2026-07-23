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
| **Perfil** | `Profile` | Conjunto nomeado de Permissões, com um Escopo (Sistema, uma Corretora ou um Tomador), que autoriza operações da plataforma a um Usuário; há Perfis fixos e Perfis customizados (proposto em 2026-07-16, cardinalidade revista em 2026-07-23 — aguardando ratificação da PO) | 1 por Usuário × Corretora e 1 por Usuário × Tomador (0..1 no Escopo Sistema) | cargo na Corretora; a Permissão isolada |
| **Permissão** | `Permission` | Autorização atômica de uma operação/funcionalidade, declarada pela própria plataforma em catálogo fixo e marcada dentro de um Perfil (proposto em 2026-07-23 — aguardando ratificação da PO) | N por Perfil | o Perfil (que agrupa Permissões) |
| **Escopo do Perfil** | `EProfileScope` | Domínio em que um Perfil vale: Sistema, uma Corretora específica ou um Tomador específico (proposto em 2026-07-23 — aguardando ratificação da PO) | 1 por Perfil | a Corretora ativa |
| **Corretor Administrador** | `BrokerageAdministrator` | Perfil fixo de Escopo Corretora que administra Usuários e Perfis da Corretora (proposto em 2026-07-23 — nome técnico e ratificação com a PO, [OPEN-09](open-decisions.md)) | — | o Corretor (papel da Pessoa) |
| **Tomador Administrador** | `PolicyHolderAdministrator` | Perfil fixo de Escopo Tomador que administra Usuários e Perfis daquele Tomador (proposto em 2026-07-23 — nome técnico e ratificação com a PO, [OPEN-09](open-decisions.md)) | — | o Tomador (papel da Pessoa) |
| **Convite** | `Invitation` | Envio, por e-mail, de um link de uso único e prazo de validade que leva o Usuário Pendente ao primeiro acesso, onde define a própria senha (proposto em 2026-07-23 — aguardando ratificação da PO) | N por Usuário enquanto Pendente | a senha inicial padrão |
| **Vínculo de Usuário com Corretora** | `UserBrokerageMembership` | Relação entre um Usuário e uma Corretora, portadora do Perfil do Usuário naquela Corretora; um Usuário pode ter vários (proposto em 2026-07-23 — aguardando ratificação da PO) | N por Usuário | o Papel da Pessoa; a Habilitação de Seguradora |
| **Corretora ativa** | `ActiveBrokerage` | A Corretora atualmente selecionada por um Usuário com vínculo em mais de uma; determina as Permissões efetivas e o Escopo das operações no momento (proposto em 2026-07-23 — aguardando ratificação da PO) | 1 por sessão | o vínculo (que é permanente) |
| **Vínculo de Usuário com Tomador** | `UserPolicyHolderMembership` | Relação entre um Usuário e um Tomador, portadora do Perfil do Usuário naquele Tomador; um Usuário pode ter vários (proposto em 2026-07-23 — aguardando ratificação da PO) | N por Usuário | o Vínculo de Usuário com Corretora; o Papel da Pessoa |
| **Tomador ativo** | `ActivePolicyHolder` | O Tomador atualmente selecionado por um Usuário com vínculo em mais de um; determina as Permissões efetivas e o Escopo das operações de contexto Tomador no momento (proposto em 2026-07-23 — aguardando ratificação da PO) | 1 por sessão | a Corretora ativa; o vínculo (permanente) |
| **Administrador do Sistema** | `SystemAdministrator` | Perfil da equipe SmartInsure que autoriza operações internas da plataforma, como manter o catálogo de Seguradoras (proposto em 2026-07-16 — aguardando ratificação da PO) | — | usuário de Corretora |
| **Pessoa** | `Person` | Pessoa física ou jurídica identificada pelo documento (CPF/CNPJ), cadastrada uma única vez na plataforma e reaproveitada pelos papéis que a referenciam (segurado, corretor, tomador); o tipo — física (`F`) ou jurídica (`J`) — deriva do documento (proposto em 2026-07-16 — aguardando ratificação da PO) | 1 por CPF/CNPJ | a Corretora e a Seguradora (têm cadastros próprios); o Usuário |
| **Papel da Pessoa** | `PersonRole` | Vínculo acumulável entre a Pessoa e um papel (Segurado `Insured`, Corretor `Broker`, Tomador `PolicyHolder`), criado automaticamente quando a Pessoa é devolvida por documento ou importada naquele contexto (proposto em 2026-07-16 — aguardando ratificação da PO) | N por Pessoa (um por papel) | o Perfil do Usuário |
| **Nome social** | `SocialName` | Nome fantasia da Pessoa jurídica ou nome social da Pessoa física, quando existir (proposto em 2026-07-16 — aguardando ratificação da PO) | 0..1 por Pessoa | o nome (razão social/nome civil) |
| **Segurado** | `Insured` | Papel da Pessoa protegida pela apólice em uma oferta (proposto em 2026-07-16 — aguardando ratificação da PO) | — | o tomador |
| **Tomador** | `PolicyHolder` | Papel da Pessoa que contrata o seguro em uma oferta; é sempre a matriz (proposto em 2026-07-16 — aguardando ratificação da PO) | — | o segurado; uma filial |
| **Natureza Jurídica** | `LegalNature` | Código oficial (CONCLA/Receita Federal) que classifica a Pessoa jurídica e determina se ela é do setor público ou privado (proposto em 2026-07-16 — aguardando ratificação da PO) | 1 por Pessoa jurídica | — |
| **Motor de Cálculo** | `CalculationEngine` | Serviço que executa as operações junto a uma Seguradora (cotar, calcular prêmio, consultar modalidades/cláusulas/coberturas, emitir, cancelar); cada Seguradora habilitada usa um motor definido na Habilitação de Seguradora — nesta fase o único motor disponível é o PlugV2 (proposto em 2026-07-19 — aguardando ratificação da PO) | 1 por Habilitação de Seguradora | a Seguradora; a integração do Birô |
| **Habilitação de Seguradora** | `BrokerageInsurerEnablement` | Vínculo entre Corretora e Seguradora que autoriza a operação do par e registra o Motor de Cálculo e os parâmetros de conexão usados (proposto em 2026-07-19 — aguardando ratificação da PO) | 0..1 por par Corretora×Seguradora | o cadastro da Seguradora; a Cotação |
| **Nomeação de Tomador** | `PolicyHolderAppointment` | Vínculo que nomeia uma Corretora para atuar por um Tomador junto a uma Seguradora; independe da Habilitação de Seguradora (proposto em 2026-07-20 — aguardando ratificação da PO) | 0..1 vigente por par Tomador×Seguradora | a Habilitação de Seguradora; o Papel da Pessoa |
| **Limite de Crédito** | `CreditLimit` | Capacidade de contratação que uma Seguradora concede a um tomador, informada por modalidade (Tradicional, Judicial, Financeiro), com taxa e validade; obtida junto à Seguradora pelo Motor de Cálculo (ratificado pela PO em 2026-07-20) | N por Consulta de Crédito (um por Seguradora×modalidade) | o limite da apólice; a Cotação |
| **Consulta de Crédito** | `CreditInquiry` | Operação em que o usuário, por uma Corretora, consulta os Limites de Crédito de um CNPJ de tomador junto às Seguradoras habilitadas; cada execução gera um registro histórico com data/hora e resultados (ratificado pela PO em 2026-07-20) | N por CNPJ (uma por execução) | a consulta ao Birô; a Oferta |
| **Matriz / Filial** | `Headquarters` / `Branch` | O estabelecimento principal da empresa (ordem `/0001` no CNPJ) / os demais estabelecimentos da mesma raiz de CNPJ (proposto em 2026-07-16 — aguardando ratificação da PO) | — | empresas distintas |

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
| **Ativo** | `Active` | Usuário que concluiu o primeiro acesso com senha própria definida | Ativo → Inativo (RN-046) |
| **Inativo** | `Inactive` | Usuário desligado do acesso; permanece no cadastro, login recusado (proposto em 2026-07-23 — aguardando ratificação da PO) | Inativo → Ativo (RN-046) |

### Seguradora (proposto em 2026-07-16 — aguardando ratificação da PO)

| Status | Nome estável (API) | Significado | Transições permitidas |
|---|---|---|---|
| **Ativa** | `Active` | Seguradora em operação no catálogo, visível aos fluxos operacionais | Ativa → Inativa (RN-009) |
| **Inativa** | `Inactive` | Seguradora fora de operação — permanece no catálogo, fora da visão operacional (nunca excluída) | Inativa → Ativa (RN-009) |

### Habilitação de Seguradora (proposto em 2026-07-19 — aguardando ratificação da PO)

| Status | Nome estável (API) | Significado | Transições permitidas |
|---|---|---|---|
| **Ativa** | `Active` | Habilitação em operação — o par Corretora×Seguradora pode operar pelo Motor de Cálculo configurado | Ativa → Inativa (RN-022) |
| **Inativa** | `Inactive` | Habilitação suspensa — permanece no cadastro, operações do par são recusadas (nunca excluída) | Inativa → Ativa (RN-022) |

### Nomeação de Tomador (proposto em 2026-07-20 — aguardando ratificação da PO)

| Status | Nome estável (API) | Significado | Transições permitidas |
|---|---|---|---|
| **Vigente** | `Active` | Nomeação em vigor — a Corretora é a nomeada do Tomador junto àquela Seguradora | Vigente → Encerrada (RN-028) |
| **Encerrada** | `Ended` | Nomeação terminada por encerramento avulso ou substituição — permanece no histórico (nunca excluída) | — |

### Corretora (ratificado pela PO em 2026-07-17)

| Status | Nome estável (API) | Significado | Transições permitidas |
|---|---|---|---|
| **Ativa** | `Active` | Corretora habilitada no cadastro de Corretoras | Ativa → Inativa (RN-021) |
| **Inativa** | `Inactive` | Corretora mantida no cadastro de Corretoras sem bloqueio automático em outros fluxos nesta fase | Inativa → Ativa (RN-021) |
