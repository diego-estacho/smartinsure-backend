# Jornada: Cotação (solicitar Cotações às Seguradoras)

Cada RN é uma seção com o ID no título e os quatro blocos abaixo. O ID é `RN-NNN` numa **sequência única do catálogo** (não reinicia por jornada), estável e nunca reaproveitado. Linguagem de negócio, termos do [glossário](../glossario.md), sem path de código no corpo.

> Escopo desta entrega: a **etapa de cotações** — solicitar as Cotações às Seguradoras a partir de um Grupo de Cotação, obtê-las e apresentá-las ao corretor, permitindo escolher uma para seguir. Ficam **fora desta fase** (demanda própria): o encaminhamento da Cotação escolhida (followup da análise de subscrição, aceite como Proposta e emissão), a Página de Listagem de Cotações e quem pode solicitar cotação por Perfil ([OPEN-03](../open-decisions.md)). Refinada em 2026-07-27; aguardando ratificação da PO ([OPEN-07](../open-decisions.md)).

## RN-052 — Solicitação de Cotações a partir do Grupo de Cotação

**Descrição.** Ao concluir a etapa de risco, o corretor solicita as Cotações do Grupo de Cotação — uma Cotação por Seguradora. A solicitação tem dois escopos: **todas as Seguradoras habilitadas** da Corretora (opção recomendada e padrão) ou **um subconjunto escolhido** pelo corretor a partir da lista de habilitadas.

**Pré-condições.** Grupo de Cotação em Rascunho (RN-050, RN-051). Corretora com ao menos uma Habilitação de Seguradora ativa.

**Critério de aceitação.** Ao entrar na etapa de cotações, a plataforma solicita Cotações conforme o escopo: no modo *todas*, a cada Seguradora habilitada ativa da Corretora; no modo *escolhidas*, exatamente às Seguradoras selecionadas, e a nenhuma outra. No modo *todas*, a solicitação inclui Seguradoras que não ofertam a Modalidade do Grupo — que retornam resultado de indisponibilidade com motivo — para que o corretor enxergue o resultado de cada Seguradora, e não apenas das que cotam.

**Casos limite.** Corretora sem Habilitação de Seguradora ativa: solicitação recusada com aviso. Escopo *escolhidas* sem nenhuma Seguradora selecionada: recusado. [ABERTO: quais Perfis podem solicitar cotação e gerenciar a Habilitação de Seguradora dependem de ratificação — OPEN-03/OPEN-07.]

## RN-053 — Cada Cotação é obtida e persistida por Seguradora, tolerando falha isolada

**Descrição.** A solicitação obtém as Cotações de forma independente por Seguradora e **persiste cada Cotação assim que a Seguradora responde**, vinculada ao Grupo de Cotação. A falha ou a demora de uma Seguradora não impede a obtenção das demais.

**Pré-condições.** Cotações solicitadas para o Grupo (RN-052).

**Critério de aceitação.** Cada retorno de Seguradora vira uma Cotação registrada no instante em que chega; o corretor acompanha o preenchimento progressivo da lista. Ao atualizar ou reabrir a etapa de cotações, o corretor vê as Cotações já obtidas, e as Seguradoras ainda pendentes seguem sendo obtidas — a solicitação é retomável, sem reiniciar do zero. Uma Seguradora que falha, fica indisponível ou excede o tempo-limite não derruba as demais: resulta numa Cotação com resultado de indisponibilidade e o motivo correspondente (RN-054).

**Casos limite.** Interrupção pelo corretor (fechar a aba, queda de conexão) não descarta as Cotações já obtidas. [ABERTO: tempo-limite por Seguradora, grau de paralelismo, política de nova tentativa e recuperação de solicitações interrompidas são parâmetros **operacionais configuráveis**, com valores padrão; um eventual teto de re-solicitações por Grupo será definido com dado de uso real — OPEN-07.]

## RN-054 — Resultado da Cotação: classificação estável, esteira e motivos

**Descrição.** Toda Cotação carrega um **resultado classificado de forma estável**: *Automático*, *Análise*, *Indisponível/Recusado* ou *Não-reconhecido*. Quando em *Análise*, a Cotação registra a **esteira** correspondente (subscrição, crédito, PEP, resseguro, cadastro). Quando *Indisponível/Recusado*, registra a **lista de motivos** informada pela Seguradora.

**Pré-condições.** Cotação obtida de uma Seguradora (RN-053).

**Critério de aceitação.** O resultado informado pela Seguradora é traduzido para uma das quatro classificações e, conforme o caso, para a esteira ou os motivos. Uma Cotação sem prêmio aplicável (Análise, Indisponível/Recusado, Não-reconhecido) não apresenta valor de prêmio. Um resultado que a plataforma **não reconhece** é classificado como *Não-reconhecido*: fica visível ao corretor identificado como não classificado, **sem prêmio, não seguível**, e é registrado para revisão — **nunca** é apresentado como *Automático* nem exibe prêmio.

**Casos limite.** Resultado novo ou desconhecido da Seguradora recai sempre em *Não-reconhecido*, jamais convertido em silêncio para outra classificação. Ausência de motivos num *Indisponível/Recusado*: apresenta indisponibilidade sem detalhamento, sem impedir as demais Cotações.

## RN-055 — Seleção da Cotação para seguir

**Descrição.** O corretor **seleciona uma** Cotação do Grupo para seguir. São seguíveis as Cotações *Automáticas* e as em *Análise* de **subscrição**; as demais classificações não são seguíveis nesta fase. A seleção **marca a Cotação escolhida**; o encaminhamento em si (followup da análise de subscrição, aceite como Proposta e emissão) é demanda posterior.

**Pré-condições.** Grupo com ao menos uma Cotação seguível.

**Critério de aceitação.** A plataforma permite marcar como escolhida uma Cotação *Automática* ou em *Análise de subscrição*. Cotações em outras esteiras de análise, *Indisponíveis/Recusadas* e *Não-reconhecidas* não podem ser escolhidas. Há no máximo uma Cotação escolhida por Grupo; escolher outra substitui a anterior.

**Casos limite.** Grupo sem nenhuma Cotação seguível: nenhuma escolha é possível. [ABERTO: o aceite da Cotação escolhida como Proposta, o followup da análise de subscrição e a emissão são demanda própria, fora desta fase — OPEN-07.]

## RN-056 — Recálculo e invalidação por mudança de dados

**Descrição.** Enquanto o Grupo está em Rascunho, **alterar qualquer dado** do Grupo invalida as Cotações já obtidas; ao reentrar na etapa de cotações, a plataforma **re-solicita** as Cotações mediante **confirmação** do corretor. Sem alteração, as Cotações exibidas são mantidas.

**Pré-condições.** Grupo com Cotações já obtidas, e retorno do corretor às etapas anteriores do wizard.

**Critério de aceitação.** Qualquer alteração de dado do Grupo (Tomador, Segurado, escopo de Seguradoras, Modalidade, valor segurado, vigência, Coberturas Adicionais) invalida as Cotações obtidas. Ao voltar à etapa de cotações com dados alterados, a plataforma pede confirmação **bloqueante** antes de re-solicitar; confirmada, re-solicita conforme RN-052/RN-053. Se nada mudou, mantém as Cotações. Havendo uma Cotação escolhida (RN-055), o recálculo **descarta a escolha** — que se referia a um risco que deixou de valer — e o corretor é informado do descarte.

**Casos limite.** O corretor recusa a confirmação de re-solicitação: a etapa mantém as Cotações anteriores, apresentadas como desatualizadas, sem re-solicitar. (A confirmação é bloqueante porque cada solicitação tem custo por chamada à Seguradora.)

## RN-057 — Validade da Cotação

**Descrição.** Uma Cotação tem **validade limitada**, definida pela Seguradora/fornecedor. Após expirar, a Cotação deixa de ser seguível e precisa ser re-solicitada.

**Pré-condições.** Cotação obtida.

**Critério de aceitação.** Uma Cotação obtida vale até o primeiro entre: (a) expirar o prazo de validade do fornecedor; (b) ser invalidada por mudança de dados (RN-056); ou (c) o Grupo seguir para as etapas posteriores. Expirada, a Cotação não é seguível e a etapa de cotações a apresenta como desatualizada, oferecendo re-solicitar.

**Casos limite.** [ABERTO: o prazo de validade (indicado como ~15 dias pelo fornecedor) e se a expiração recai sobre a Cotação ou sobre a Proposta dependem de confirmação no contrato do fornecedor e de ratificação da PO — OPEN-07.]
