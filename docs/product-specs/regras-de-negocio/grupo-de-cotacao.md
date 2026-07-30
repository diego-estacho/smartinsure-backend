# Jornada: Grupo de Cotação (nova oferta)

Cada RN é uma seção com o ID no título e os quatro blocos abaixo. O ID é `RN-NNN` numa **sequência única do catálogo** (não reinicia por jornada), estável e nunca reaproveitado. Linguagem de negócio, termos do [glossário](../glossario.md), sem path de código no corpo.

> Escopo desta jornada: **o Grupo de Cotação em Rascunho** — o pedido que o corretor monta no wizard até concluir a etapa de risco, incluindo o estabelecimento cotado (RN-102). Solicitar as Cotações às Seguradoras é a jornada [Cotação](cotacao.md) (RN-056..RN-063, ratificada em 2026-07-28); a **emissão** segue fora de escopo, em demanda própria ([OPEN-07](../open-decisions.md)).

## RN-050 — Criação do Grupo de Cotação

**Descrição.** Ao concluir a etapa de dados de risco do wizard de nova oferta, a plataforma persiste um Grupo de Cotação em Rascunho, reunindo o que o corretor montou até ali: o Tomador, o estabelecimento cotado (RN-102), o Segurado, o escopo de Seguradoras a cotar, a Modalidade, o valor segurado, a vigência e as Coberturas Adicionais marcadas. Existe um único Grupo de Cotação por jornada; enquanto Rascunho, revisões das etapas anteriores atualizam o mesmo grupo (RN-051), nunca criam um novo.

**Pré-condições.** Usuário autenticado por uma Corretora (nesta fase, sem restrição de Perfil — OPEN-03). Tomador e Segurado já existentes como Papéis de Pessoa (RN-017). Modalidade Ativa no catálogo do Smart.

**Critério de aceitação.** Concluída a etapa de risco com Tomador, Segurado, Modalidade, valor segurado e vigência informados, a plataforma cria o Grupo de Cotação em Rascunho e devolve seu identificador. O grupo referencia o Tomador e o Segurado pelo Papel de Pessoa e a Modalidade pelo catálogo do Smart; guarda o estabelecimento cotado quando houver Filial escolhida (RN-102), o escopo de Seguradoras a cotar, o valor segurado, a vigência (início e fim) e as Coberturas Adicionais marcadas. Nenhuma Cotação de Seguradora é solicitada nesta etapa — o Rascunho apenas registra o pedido do corretor.

**Casos limite.** Falta de Tomador, Segurado, Modalidade, valor segurado ou vigência: criação recusada (a validação de forma é do wizard; a de negócio é do servidor — ADR-004). Vigência com fim anterior ao início: recusada. Referência a Tomador, Segurado ou Modalidade inexistente: recusada. O escopo de Seguradoras foi fechado pela RN-056 (modo *todas* × *escolhidas*; escopo *escolhidas* vazio é recusado). [ABERTO: limites do valor segurado e demais validações de negócio dependem de ratificação da PO — OPEN-07.]

## RN-051 — Atualização do Grupo de Cotação em Rascunho

**Descrição.** Enquanto o Grupo de Cotação está em Rascunho, o corretor volta livremente às etapas de tomador, segurado e risco e altera os dados; ao prosseguir da etapa de risco, a plataforma atualiza o **mesmo** Grupo de Cotação, mantendo o identificador — nunca cria um novo. Alterar o valor de um dado-base do Grupo invalida as Cotações já obtidas: a regra de invalidação e re-solicitação é a [RN-060](cotacao.md), fonte única da lista de dados-base — o estabelecimento cotado (RN-102) é um deles.

**Pré-condições.** Grupo de Cotação existente em Rascunho, criado na mesma jornada (RN-050).

**Critério de aceitação.** Ao prosseguir da etapa de risco com um Grupo de Cotação já existente na jornada, a plataforma atualiza o registro corrente com os dados informados e devolve o mesmo identificador. O grupo permanece em Rascunho após a atualização.

**Casos limite.** A lista de dados-base cuja mudança invalida as Cotações foi fechada pela RN-060 (ratificada em 2026-07-28) — não está mais em aberto. Uma mudança que não altera valor não invalida nada. O comportamento a partir da etapa de emissão segue fora de escopo ([OPEN-07](../open-decisions.md)).

## RN-102 — Estabelecimento cotado no Grupo de Cotação

**Descrição.** O Grupo de Cotação registra qual estabelecimento do Tomador é o objeto da garantia: a matriz ou uma das suas Filiais (RN-101). O Tomador do grupo continua sendo sempre a matriz (RN-016); a Filial é uma escolha do corretor dentro dela, no máximo uma por Grupo de Cotação. É o estabelecimento cotado que identifica a garantia junto à Seguradora: o CNPJ enviado ao cotar e o CNPJ da apólice emitida são os dele. Já o Limite de Crédito e a taxa são sempre da matriz.

**Pré-condições.** Jornada de nova oferta com Tomador selecionado. Filiais do Tomador cadastradas (RN-101), quando houver escolha a fazer.

**Critério de aceitação.** Na etapa de tomador, as Filiais do Tomador são apresentadas em lista e o corretor marca no máximo uma — marcar outra desmarca a anterior. Com uma Filial marcada, o Grupo de Cotação registra essa Filial como estabelecimento cotado e o resumo da jornada apresenta o CNPJ dela; sem nenhuma marcada, o estabelecimento cotado é a matriz e o resumo apresenta o CNPJ da matriz. Quando o corretor chega ao Tomador informando o CNPJ de uma Filial, essa Filial nasce marcada; nos demais casos a lista abre desmarcada. Ao retomar um Grupo de Cotação em Rascunho que já registrou uma Filial, ela volta marcada.

**Casos limite.** Filial inexistente, ou vinculada a outra matriz que não o Tomador do grupo: recusada — a validação de vínculo é do servidor. Troca do Tomador do Grupo de Cotação: o estabelecimento cotado é limpo e volta a ser a matriz. Trocar ou desmarcar a Filial invalida as Cotações já obtidas — o estabelecimento cotado é dado-base (RN-060). Grupo de Cotação criado antes desta regra permanece válido, com a matriz como estabelecimento cotado. **Limite de Crédito e taxa são sempre da matriz**, marcada ou não uma Filial: a Seguradora não consulta limite pelo CNPJ da Filial — a escolha do estabelecimento não altera os limites apresentados (RN-029). Que a Seguradora avalie o risco com base na matriz é funcionamento interno dela; a plataforma envia o estabelecimento cotado. A **emissão** pelo CNPJ do estabelecimento segue fora de escopo, na demanda da etapa de emissão ([OPEN-07](../open-decisions.md)).

> **Pendência de implementação (não é decisão aberta).** A solicitação de Cotações (RN-056/RN-057) hoje envia à Seguradora o CNPJ do Tomador — a matriz — mesmo com Filial marcada; a etapa de cotações foi construída antes desta regra existir. Enquanto isso não for corrigido, a Filial é registrada e exibida, mas não chega à Seguradora. A regra acima é a decidida; falta implementá-la no envio ao provedor.
