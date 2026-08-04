# Jornada: Consulta de Crédito

Cada RN é uma seção com o ID no título e os quatro blocos abaixo. O ID é `RN-NNN` numa **sequência única do catálogo** (não reinicia por jornada), estável e nunca reaproveitado. Linguagem de negócio, termos do [glossário](../glossario.md), sem path de código no corpo.

## RN-029 — Consulta de Limites de Crédito do Tomador

**Descrição.** O usuário informa a Corretora e o CNPJ do tomador, e a plataforma consulta os Limites de Crédito daquele tomador junto a cada Seguradora com Habilitação de Seguradora Ativa da Corretora, simultaneamente, pelo Motor de Cálculo resolvido pela Habilitação (RN-023). O resultado é apresentado agrupado por Seguradora.

**Pré-condições.** Usuário autenticado na plataforma (nesta fase, sem restrição de Perfil). Corretora Ativa selecionada pelo usuário na tela (o vínculo automático Usuário×Corretora segue aberto — OPEN-03). Ao menos uma Habilitação de Seguradora Ativa para a Corretora.

**Critério de aceitação.** Dado um CNPJ válido, a plataforma dispara uma consulta por Seguradora com Habilitação Ativa da Corretora e apresenta, por Seguradora: o status do retorno, o tempo de resposta da Seguradora e os Limites de Crédito agrupados pelo grupo de modalidade informado pela própria Seguradora (ex.: Tradicional, Judiciais, Financeira), cada grupo com o limite disponível, o limite utilizado (diferença entre o limite revisado e o disponível) e a taxa — o valor do grupo é o maior limite entre as modalidades que o compõem. Um resumo consolida a quantidade de Seguradoras consultadas, quantas aprovaram e o limite consolidado. O CNPJ não precisa estar cadastrado como Tomador na plataforma — a consulta é feita diretamente pelo documento informado; quando a Seguradora informar a razão social do tomador, ela é apresentada.

**Casos limite.** CNPJ inválido é recusado antes de qualquer consulta. Corretora sem nenhuma Habilitação Ativa: a consulta é recusada com mensagem indicando a ausência de Seguradoras habilitadas. Grupo de modalidade não retornado pela Seguradora é apresentado como ausente, sem inventar valor. A validade do limite não tem fonte no motor nesta fase (OPEN-08) — apresentada como ausente, nunca inventada. O tempo de resposta é a duração medida da consulta àquela Seguradora; quando a Seguradora não responde, é apresentado como ausente, nunca estimado.

## RN-030 — Falha isolada na Consulta de Crédito

**Descrição.** Falha do Motor de Cálculo ao consultar o Limite de Crédito junto a uma Seguradora torna indisponível apenas o resultado daquela Seguradora, sem impedir a Consulta de Crédito nem os resultados das demais Seguradoras habilitadas.

**Pré-condições.** Consulta de Crédito disparada (RN-029) para uma ou mais Seguradoras.

**Critério de aceitação.** Quando o Motor de Cálculo falha para uma Seguradora (indisponibilidade, erro ou tempo excedido), o resultado daquela Seguradora é apresentado como indisponível com o motivo, e as demais Seguradoras seguem seu fluxo normalmente. O resumo consolidado considera apenas as Seguradoras que responderam.

**Casos limite.** Falha em todas as Seguradoras habilitadas: a Consulta de Crédito permanece válida, sem nenhum resultado disponível. Resposta do motor em formato inesperado é tratada como falha, nunca como resultado válido.

## RN-031 — Histórico de Consultas de Crédito

**Descrição.** Cada Consulta de Crédito concluída é registrada com data e hora, Corretora, CNPJ consultado e o resultado obtido por Seguradora (inclusive indisponibilidades e seus motivos). A consulta é sempre online — o histórico serve à rastreabilidade, nunca como reuso de resposta.

**Pré-condições.** Consulta de Crédito executada (RN-029).

**Critério de aceitação.** Ao concluir a Consulta de Crédito, a plataforma grava o registro com data e hora, Corretora, CNPJ e o resultado por Seguradora — incluindo o tempo de resposta de cada Seguradora. Reconsultar o mesmo CNPJ dispara novas consultas às Seguradoras e gera um novo registro, sem alterar os anteriores. Os registros ficam disponíveis para consulta posterior.

**Casos limite.** Consulta com falhas (RN-030) é registrada da mesma forma, com os motivos de indisponibilidade. Registro de Consulta de Crédito nunca é editado nem excluído. Consulta recusada antes do disparo (CNPJ inválido, Corretora sem Habilitação Ativa) não gera registro.

## RN-104 — Busca de Tomador para a Consulta de Crédito

**Descrição.** Na Consulta de Crédito, o usuário pode localizar o Tomador por texto livre (razão social, nome ou CNPJ) antes de disparar a consulta. A plataforma retorna os Tomadores cadastrados que correspondem ao termo, para que o usuário escolha um e consulte. Para cada candidato, é indicado se ele já é Tomador da Corretora ativa. É uma conveniência de localização — a Consulta de Crédito em si continua sendo feita pelo CNPJ (RN-029), que não exige cadastro prévio.

**Pré-condições.** Usuário autenticado; Corretora ativa selecionada.

**Critério de aceitação.** Dado um termo de busca, a plataforma apresenta os Tomadores cadastrados correspondentes, cada um com razão social, CNPJ, cidade e UF, e a indicação "já é Tomador desta Corretora" quando existe Nomeação Vigente (Active) do Tomador com a Corretora ativa (RN-027/028). Escolher um candidato dispara a Consulta de Crédito (RN-029) pelo CNPJ do Tomador. Informar um CNPJ válido pode dispensar a lista e disparar a consulta diretamente (RN-029).

**Casos limite.** Termo sem correspondência não retorna candidatos (a tela orienta informar o CNPJ). Cidade e UF ausentes no cadastro do Tomador são apresentadas como ausentes, nunca inventadas. A indicação "já é Tomador desta Corretora" reflete apenas Nomeação Vigente; Nomeação encerrada (Ended) não conta. A busca é somente leitura — não cria vínculo nem Nomeação.
