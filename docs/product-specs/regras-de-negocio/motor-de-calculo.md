# Jornada: Motor de Cálculo

Cada RN é uma seção com o ID no título e os quatro blocos abaixo. O ID é `RN-NNN` numa **sequência única do catálogo** (não reinicia por jornada), estável e nunca reaproveitado. Linguagem de negócio, termos do [glossário](../glossario.md), sem path de código no corpo.

## RN-022 — Habilitação de Seguradora para a Corretora

**Descrição.** A Corretora opera com uma Seguradora somente através de uma Habilitação de Seguradora, que registra qual Motor de Cálculo aquele par usa e os parâmetros de conexão necessários para aquele motor.

**Pré-condições.** Usuário autenticado na plataforma (nesta fase, sem restrição de Perfil). Corretora e Seguradora existentes nos respectivos cadastros.

**Critério de aceitação.** O usuário autenticado cria, consulta e altera a Habilitação de Seguradora informando a Corretora, a Seguradora, o Motor de Cálculo e os parâmetros de conexão exigidos pelo motor escolhido. Existe no máximo uma Habilitação por par Corretora×Seguradora. A Habilitação nasce Ativa e pode ser alternada entre Ativa e Inativa; nunca é excluída.

**Casos limite.** Tentativa de criar segunda Habilitação para o mesmo par Corretora×Seguradora é recusada. Habilitação sem Motor de Cálculo informado é recusada. Parâmetros de conexão obrigatórios do motor ausentes recusam a gravação. Habilitação Inativa permanece consultável no cadastro.

## RN-023 — Seleção do Motor de Cálculo pela Habilitação

**Descrição.** Toda operação da plataforma junto a uma Seguradora (cotar, calcular prêmio, consultar modalidades, cláusulas, coberturas, emitir, cancelar) resolve qual Motor de Cálculo usar pela Habilitação de Seguradora Ativa do par Corretora×Seguradora — nunca por escolha fixa no código.

**Pré-condições.** Operação solicitada em nome de uma Corretora para uma Seguradora.

**Critério de aceitação.** Dada uma Habilitação Ativa do par com Motor de Cálculo configurado, a operação é executada por esse motor com os parâmetros de conexão da Habilitação. Sem Habilitação, com Habilitação Inativa ou sem Motor de Cálculo resolvível, a operação é recusada com mensagem indicando que a Seguradora não está habilitada para a Corretora.

**Casos limite.** Seguradora Inativa no catálogo: a operação é recusada mesmo com Habilitação Ativa (RN-010). Motor de Cálculo referenciado pela Habilitação não disponível na plataforma: operação recusada, sem tentativa por outro motor. Nesta fase o único Motor de Cálculo disponível é o PlugV2.

## RN-024 — Falha do Motor de Cálculo não derruba a Oferta

**Descrição.** Falha do Motor de Cálculo ao obter a Cotação de uma Seguradora torna indisponível apenas a Cotação daquela Seguradora, sem impedir a Oferta nem as Cotações das demais Seguradoras habilitadas.

**Pré-condições.** Operação de cotação executada através de um Motor de Cálculo resolvido pela Habilitação (RN-023).

**Critério de aceitação.** Quando o Motor de Cálculo falha (indisponibilidade, erro ou tempo excedido), a Cotação daquela Seguradora é registrada como indisponível com o motivo, a Oferta permanece válida e as demais Seguradoras habilitadas seguem seu fluxo normalmente.

**Casos limite.** Falha em todas as Seguradoras habilitadas: a Oferta permanece válida, sem nenhuma Cotação disponível. Resposta do motor em formato inesperado é tratada como falha, nunca como Cotação válida.
