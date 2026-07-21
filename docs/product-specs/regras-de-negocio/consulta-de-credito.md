# Jornada: Consulta de Crédito

Cada RN é uma seção com o ID no título e os quatro blocos abaixo. O ID é `RN-NNN` numa **sequência única do catálogo** (não reinicia por jornada), estável e nunca reaproveitado. Linguagem de negócio, termos do [glossário](../glossario.md), sem path de código no corpo.

## RN-029 — Consulta de Limites de Crédito do Tomador

**Descrição.** O usuário informa a Corretora e o CNPJ do tomador, e a plataforma consulta os Limites de Crédito daquele tomador junto a cada Seguradora com Habilitação de Seguradora Ativa da Corretora, simultaneamente, pelo Motor de Cálculo resolvido pela Habilitação (RN-023). O resultado é apresentado agrupado por Seguradora.

**Pré-condições.** Usuário autenticado na plataforma (nesta fase, sem restrição de Perfil). Corretora Ativa selecionada pelo usuário na tela (o vínculo automático Usuário×Corretora segue aberto — OPEN-03). Ao menos uma Habilitação de Seguradora Ativa para a Corretora.

**Critério de aceitação.** Dado um CNPJ válido, a plataforma dispara uma consulta por Seguradora com Habilitação Ativa da Corretora e apresenta, por Seguradora: o status do retorno e os Limites de Crédito agrupados pelo grupo de modalidade informado pela própria Seguradora (ex.: Tradicional, Judiciais, Financeira), cada grupo com o limite disponível, o limite utilizado (diferença entre o limite revisado e o disponível) e a taxa — o valor do grupo é o maior limite entre as modalidades que o compõem. Um resumo consolida a quantidade de Seguradoras consultadas, quantas aprovaram e o limite consolidado. O CNPJ não precisa estar cadastrado como Tomador na plataforma — a consulta é feita diretamente pelo documento informado; quando a Seguradora informar a razão social do tomador, ela é apresentada.

**Casos limite.** CNPJ inválido é recusado antes de qualquer consulta. Corretora sem nenhuma Habilitação Ativa: a consulta é recusada com mensagem indicando a ausência de Seguradoras habilitadas. Grupo de modalidade não retornado pela Seguradora é apresentado como ausente, sem inventar valor. A validade do limite não tem fonte no motor nesta fase (OPEN-08) — apresentada como ausente, nunca inventada.

## RN-030 — Falha isolada na Consulta de Crédito

**Descrição.** Falha do Motor de Cálculo ao consultar o Limite de Crédito junto a uma Seguradora torna indisponível apenas o resultado daquela Seguradora, sem impedir a Consulta de Crédito nem os resultados das demais Seguradoras habilitadas.

**Pré-condições.** Consulta de Crédito disparada (RN-029) para uma ou mais Seguradoras.

**Critério de aceitação.** Quando o Motor de Cálculo falha para uma Seguradora (indisponibilidade, erro ou tempo excedido), o resultado daquela Seguradora é apresentado como indisponível com o motivo, e as demais Seguradoras seguem seu fluxo normalmente. O resumo consolidado considera apenas as Seguradoras que responderam.

**Casos limite.** Falha em todas as Seguradoras habilitadas: a Consulta de Crédito permanece válida, sem nenhum resultado disponível. Resposta do motor em formato inesperado é tratada como falha, nunca como resultado válido.

## RN-031 — Histórico de Consultas de Crédito

**Descrição.** Cada Consulta de Crédito concluída é registrada com data e hora, Corretora, CNPJ consultado e o resultado obtido por Seguradora (inclusive indisponibilidades e seus motivos). A consulta é sempre online — o histórico serve à rastreabilidade, nunca como reuso de resposta.

**Pré-condições.** Consulta de Crédito executada (RN-029).

**Critério de aceitação.** Ao concluir a Consulta de Crédito, a plataforma grava o registro com data e hora, Corretora, CNPJ e o resultado por Seguradora. Reconsultar o mesmo CNPJ dispara novas consultas às Seguradoras e gera um novo registro, sem alterar os anteriores. Os registros ficam disponíveis para consulta posterior.

**Casos limite.** Consulta com falhas (RN-030) é registrada da mesma forma, com os motivos de indisponibilidade. Registro de Consulta de Crédito nunca é editado nem excluído. Consulta recusada antes do disparo (CNPJ inválido, Corretora sem Habilitação Ativa) não gera registro.
