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

## RN-200 — Busca de Tomador para a Consulta de Crédito

**Descrição.** Na Consulta de Crédito, o usuário pode localizar o Tomador por texto livre (razão social, nome ou CNPJ) antes de disparar a consulta. A plataforma retorna os Tomadores cadastrados que correspondem ao termo, para que o usuário escolha um e consulte. Para cada candidato, é indicado se ele já é Tomador da Corretora ativa. É uma conveniência de localização — a Consulta de Crédito em si continua sendo feita pelo CNPJ (RN-029), que não exige cadastro prévio.

**Pré-condições.** Usuário autenticado; Corretora ativa selecionada.

**Critério de aceitação.** Dado um termo de busca, a plataforma apresenta os Tomadores cadastrados correspondentes, cada um com razão social, CNPJ, cidade e UF, e a indicação "já é Tomador desta Corretora" quando existe Nomeação Vigente (Active) do Tomador com a Corretora ativa (RN-027/028). Escolher um candidato dispara a Consulta de Crédito (RN-029) pelo CNPJ do Tomador. Informar um CNPJ válido pode dispensar a lista e disparar a consulta diretamente (RN-029).

**Casos limite.** Termo sem correspondência não retorna candidatos (a tela orienta informar o CNPJ). Cidade e UF ausentes no cadastro do Tomador são apresentadas como ausentes, nunca inventadas. A indicação "já é Tomador desta Corretora" reflete apenas Nomeação Vigente; Nomeação encerrada (Ended) não conta. A busca é somente leitura — não cria vínculo nem Nomeação.

## RN-201 — Exportação da Consulta de Crédito

> **Rascunho — aprovação da PO pendente.** Catalogada junto ao design homologado da camada 2 (exec-plan 0017); segue o precedente da exportação de Corretoras (RN-018).

**Descrição.** O usuário pode exportar o quadro consolidado de uma Consulta de Crédito concluída para uma planilha (.xlsx), com uma linha por Seguradora, para trabalhar os limites fora da plataforma. A exportação reflete fielmente o resultado registrado da consulta (RN-031) — não dispara nova consulta às Seguradoras.

**Pré-condições.** Consulta de Crédito concluída e registrada (RN-029/RN-031); usuário autenticado.

**Critério de aceitação.** A partir de uma Consulta de Crédito concluída, a plataforma gera uma planilha com uma linha por Seguradora, na mesma ordenação da tela (Aprovado antes de Indisponível; dentro do grupo, por maior limite disponível). Cada linha traz a Seguradora, o status (Aprovado/Indisponível), o limite e a taxa de cada grupo de modalidade nas colunas fixas (Tradicional, Judicial, Financeira), o limite utilizado, o tempo de resposta e o motivo da indisponibilidade quando houver. O Tomador, o CNPJ e a data da consulta identificam o arquivo. A planilha é gerada pela mesma capacidade de exportação já usada em outras listagens (RN-018).

**Casos limite.** Grupo de modalidade não retornado pela Seguradora é apresentado como ausente na planilha, nunca inventado (consistente com RN-029). Seguradora indisponível (RN-030) aparece na planilha com o status e o motivo, sem limites. Consulta de Crédito inexistente não gera arquivo. A exportação é somente leitura — nunca altera o registro da consulta (RN-031).
