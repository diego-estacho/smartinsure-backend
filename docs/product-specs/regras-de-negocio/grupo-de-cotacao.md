# Jornada: Grupo de Cotação (nova oferta)

Cada RN é uma seção com o ID no título e os quatro blocos abaixo. O ID é `RN-NNN` numa **sequência única do catálogo** (não reinicia por jornada), estável e nunca reaproveitado. Linguagem de negócio, termos do [glossário](../glossario.md), sem path de código no corpo.

> Escopo desta entrega: **persistir o Grupo de Cotação em Rascunho** — o pedido que o corretor monta no wizard até concluir a etapa de risco. Solicitar as Cotações às Seguradoras (etapa de cotações) e a emissão (etapa de emissão) seguem fora de escopo nesta fase e permanecem mockadas no front ([OPEN-07](../open-decisions.md)).

## RN-050 — Criação do Grupo de Cotação

**Descrição.** Ao concluir a etapa de dados de risco do wizard de nova oferta, a plataforma persiste um Grupo de Cotação em Rascunho, reunindo o que o corretor montou até ali: o Tomador, o Segurado, o escopo de Seguradoras a cotar, a Modalidade, o valor segurado, a vigência e as Coberturas Adicionais marcadas. Existe um único Grupo de Cotação por jornada; enquanto Rascunho, revisões das etapas anteriores atualizam o mesmo grupo (RN-051), nunca criam um novo.

**Pré-condições.** Usuário autenticado por uma Corretora (nesta fase, sem restrição de Perfil — OPEN-03). Tomador e Segurado já existentes como Papéis de Pessoa (RN-017). Modalidade Ativa no catálogo do Smart.

**Critério de aceitação.** Concluída a etapa de risco com Tomador, Segurado, Modalidade, valor segurado e vigência informados, a plataforma cria o Grupo de Cotação em Rascunho e devolve seu identificador. O grupo referencia o Tomador e o Segurado pelo Papel de Pessoa e a Modalidade pelo catálogo do Smart; guarda o escopo de Seguradoras a cotar, o valor segurado, a vigência (início e fim) e as Coberturas Adicionais marcadas. Nenhuma Cotação de Seguradora é solicitada nesta etapa — o Rascunho apenas registra o pedido do corretor.

**Casos limite.** Falta de Tomador, Segurado, Modalidade, valor segurado ou vigência: criação recusada (a validação de forma é do wizard; a de negócio é do servidor — ADR-004). Vigência com fim anterior ao início: recusada. Referência a Tomador, Segurado ou Modalidade inexistente: recusada. [ABERTO: limites do valor segurado, escopo mínimo de Seguradoras e demais validações de negócio dependem de ratificação da PO — OPEN-07.]

## RN-051 — Atualização do Grupo de Cotação em Rascunho

**Descrição.** Enquanto o Grupo de Cotação está em Rascunho, o corretor volta livremente às etapas de tomador, segurado e risco e altera os dados; ao prosseguir da etapa de risco, a plataforma atualiza o **mesmo** Grupo de Cotação, mantendo o identificador — nunca cria um novo. Alterar dados que alimentam a cotação (escopo de Seguradoras, Modalidade, valor segurado, vigência, Coberturas Adicionais) invalida as Cotações eventualmente já obtidas, que são recalculadas ao reentrar na etapa de cotações.

**Pré-condições.** Grupo de Cotação existente em Rascunho, criado na mesma jornada (RN-050).

**Critério de aceitação.** Ao prosseguir da etapa de risco com um Grupo de Cotação já existente na jornada, a plataforma atualiza o registro corrente com os dados informados e devolve o mesmo identificador. O grupo permanece em Rascunho após a atualização.

**Casos limite.** [ABERTO: o conjunto exato de campos que "alimentam a cotação" (assinatura de recálculo) e o comportamento a partir da etapa de cotações/emissão dependem de ratificação da PO — OPEN-07. Nesta fase o backend persiste apenas o Rascunho; a invalidação/recálculo das Cotações e a emissão são comportamento do front, ainda mockados.]
