# Jornada: Catálogo de Modalidades

Cada RN é uma seção com o ID no título e os quatro blocos abaixo. O ID é `RN-NNN` numa **sequência única do catálogo** (não reinicia por jornada), estável e nunca reaproveitado. Linguagem de negócio, termos do [glossário](../glossario.md), sem path de código no corpo.

> Origem: especificação de produto "Catálogo de Modalidades" (2026-07-21, AB#0002). Status: **proposta** — aguardando refino/ratificação da PO. Nada aqui é implementável antes da aprovação. A automação de mapeamento "por semelhança" está fora desta fase e registrada em [open-decisions.md](../open-decisions.md).

## RN-029 — Modalidade e Grupo são catálogo curado

**Descrição.** Modalidade e Grupo de Modalidade formam um catálogo curado pela equipe SmartInsure: nascem por decisão humana explícita (curadoria direta ou promoção de uma pendência da Fila), nunca criados automaticamente pela importação. A escrita no catálogo — criar, editar e inativar Modalidade e Grupo — é restrita ao Administrador do Sistema.

**Pré-condições.** Usuário autenticado com Perfil Administrador do Sistema (mesmo padrão de escrita do catálogo de Seguradoras, RN-011).

**Critério de aceitação.** O Administrador do Sistema cria, edita e inativa Grupo de Modalidade (nome, descrição, situação, ordem de exibição) e Modalidade (nome, grupo, descrição, situação). O nome da Modalidade é único no catálogo e toda Modalidade pertence a um Grupo. Usuário sem o Perfil não escreve no catálogo. A importação nunca cria Modalidade nem Grupo de Modalidade — apenas o lado importado (RN-030).

**Casos limite.** Nome de Modalidade duplicado é recusado. Modalidade sem Grupo é recusada. Inativar um Grupo esconde o Grupo e suas Modalidades da operação sem apagar nada (RN-036).

## RN-030 — Modalidade Importada reflete a fonte

**Descrição.** Cada Modalidade Importada reflete sempre a última importação bem-sucedida da Seguradora: nome de origem, público-alvo, ramo, grupo importado e parâmetros comerciais vêm da Seguradora e não são editados à mão na plataforma. Entre importações, a Modalidade Importada é reencontrada pelo identificador de origem, preservando sua identidade e o mapeamento.

**Pré-condições.** Importação bem-sucedida de uma Seguradora (RN-031).

**Critério de aceitação.** Uma modalidade trazida ainda não conhecida (pelo identificador de origem) é criada como Modalidade Importada com os dados da Seguradora; já conhecida é atualizada com os dados atuais, mantendo sua identidade e seu mapeamento. A plataforma não permite editar nome, público-alvo, ramo ou parâmetros comerciais da Modalidade Importada à mão. A data da última importação é registrada.

**Casos limite.** A Seguradora renomeia a modalidade: reconhecida pelo identificador de origem, atualiza o nome e mantém o mapeamento — não vira item novo na Fila. Identificador de origem ausente na resposta é tratado como dado inválido: não cria item órfão.

## RN-031 — Importação periódica por Seguradora, motor resolvido pela Habilitação

**Descrição.** A importação de modalidades roda periodicamente e percorre as Seguradoras com Habilitação de Seguradora Ativa e Motor de Cálculo configurado, com uma consulta por Seguradora. O Motor de Cálculo e os parâmetros de conexão são resolvidos pela Habilitação (RN-023), nunca fixos no código.

**Pré-condições.** Habilitação de Seguradora Ativa com Motor de Cálculo configurado (RN-022).

**Critério de aceitação.** A cada execução agendada, para cada Seguradora habilitada, a importação obtém o catálogo de modalidades daquela Seguradora pelo Motor de Cálculo resolvido na Habilitação e alimenta o lado importado (RN-030). Seguradora sem Habilitação Ativa ou sem Motor de Cálculo configurado não é consultada.

**Casos limite.** A mesma Seguradora habilitada por várias Corretoras é consultada uma única vez, deduplicada por Seguradora — qual credencial é usada e se catálogos podem divergir entre Corretoras é decisão aberta (ver open-decisions). Seguradora Inativa no catálogo não é importada (RN-010). Sem Motor de Cálculo resolvível, a Seguradora não é importada e fica registrada como não processada.

## RN-032 — Mapeamento automático por identificador do motor, dentro do mesmo ramo

**Descrição.** Na importação, uma Modalidade Importada é mapeada automaticamente para uma Modalidade apenas quando o Motor de Cálculo fornece um identificador de modalidade que já aponta para uma Modalidade, e sempre dentro do mesmo Ramo. Nenhuma outra forma de mapeamento é automática nesta fase.

**Pré-condições.** Modalidade Importada atualizada pela importação (RN-030), com o identificador do motor conhecido.

**Critério de aceitação.** Existindo mapeamento Confirmado de outra Modalidade Importada com o mesmo identificador do motor apontando para uma Modalidade, o mapeamento da Modalidade Importada atual é criado Confirmado e marcado como estabelecido por identificador, para a mesma Modalidade. O Ramo da Modalidade Importada precisa coincidir com o Ramo das Modalidades Importadas já confirmadas naquela Modalidade; nenhum mapeamento automático cruza Ramos. Um mapeamento válido preexistente da Modalidade Importada conhecida é reaproveitado.

**Casos limite.** Identificador do motor que ainda não aponta para nenhuma Modalidade: não confirma, o item segue para a Fila de Revisão (RN-034). Ramo incompatível com o já fixado para a Modalidade candidata: não mapeia, vai para a Fila. Mapeamento por semelhança não ocorre nesta fase (decisão aberta).

## RN-033 — Nada entra na operação sem mapeamento confirmado

**Descrição.** O que o corretor vê é sempre a Modalidade. Uma Modalidade só é oferecida enquanto tiver ao menos uma Modalidade Importada Ativa com mapeamento Confirmado. Duas ofertas só são "a mesma modalidade em Seguradoras diferentes" quando ambas as Modalidades Importadas têm mapeamento Confirmado para a mesma Modalidade. A disponibilidade da Modalidade por tipo de tomador é derivada, nunca digitada.

**Pré-condições.** Catálogo de Modalidades e de Modalidades Importadas alimentado; mapeamentos resolvidos (RN-032, RN-034).

**Critério de aceitação.** Modalidade sem nenhuma Modalidade Importada Ativa com mapeamento Confirmado não aparece na operação, mas permanece no catálogo. Modalidade Importada com mapeamento Pendente, ausente ou Ignorado não é oferecida. A disponibilidade da Modalidade (pessoa física/jurídica, ente público/privado) é calculada a partir das Modalidades Importadas Ativas confirmadas mapeadas para ela. A comparação entre Seguradoras usa a Modalidade como eixo.

**Casos limite.** A última Seguradora que oferecia a Modalidade sai: a Modalidade some da operação e permanece no catálogo. Se nenhuma Modalidade Importada Ativa a oferece para pessoa física, a Modalidade não aparece para pessoa física. Os parâmetros comerciais de cada Seguradora afloram somente quando o corretor escolhe cotar naquela Seguradora.

## RN-034 — Fila de Revisão: mapear, promover ou ignorar

**Descrição.** As Modalidades Importadas que a importação não conseguiu mapear com segurança compõem a Fila de Revisão — o recorte "precisa de decisão" evidenciado dentro do Mapa de Modalidades. É o único ponto humano do fluxo de mapeamento: a equipe resolve cada pendência mapeando para uma Modalidade existente, promovendo (criando uma Modalidade nova e mapeando) ou ignorando a Modalidade Importada.

**Pré-condições.** Modalidade Importada com mapeamento Pendente ou ausente.

**Critério de aceitação.** No Mapa, as pendências ficam evidenciadas na própria exibição. Mapear confirma o mapeamento, marcado como manual, com registro de quem confirmou e quando. Promover cria a Modalidade na tela de cadastro (RN-029) e mapeia a pendência a ela. Ignorar marca a Modalidade Importada como Ignorada: não é oferecida e não volta à Fila nas próximas importações, mas fica registrada. Enquanto pendente, a Modalidade Importada não é oferecida (RN-033).

**Casos limite.** Item ignorado permanece registrado e pode ser reavaliado depois. Promover respeita o nome único da Modalidade (RN-029). Mapear para uma Modalidade de Ramo incompatível com a Modalidade Importada é recusado (trava de ramo).

## RN-035 — Resiliência da importação

**Descrição.** Falha de uma Seguradora numa importação não desativa nada dela nem impede as demais. Apenas a ausência de uma Modalidade Importada numa importação bem-sucedida daquela Seguradora a desativa.

**Pré-condições.** Execução da importação (RN-031).

**Critério de aceitação.** Erro, tempo excedido ou resposta inválida de uma Seguradora é registrado como falha daquela Seguradora, sem desativar suas Modalidades Importadas e sem afetar as demais Seguradoras. Numa importação bem-sucedida, as Modalidades Importadas que estavam Ativas e não vieram passam a Inativas. Reaparecendo numa importação futura, são reativadas com o mapeamento anterior, se ainda válido.

**Casos limite.** Todas as Seguradoras falham: nada é desativado, o catálogo permanece intacto. Resposta em formato inesperado é tratada como falha, nunca como catálogo vazio — não desativa nada por engano.

## RN-036 — Preservação do catálogo

**Descrição.** Nada no catálogo de Modalidades é apagado. Modalidades, Grupos de Modalidade, Modalidades Importadas e Grupos Importados saem de operação por Inativação, mantendo histórico e permitindo retorno.

**Pré-condições.** Item existente no catálogo.

**Critério de aceitação.** Inativar um item o retira da operação sem removê-lo; um item Inativo permanece consultável e pode ser reativado. A importação usa Inativação (nunca exclusão) para o que deixou de vir (RN-035).

**Casos limite.** Modalidade sem nenhuma Seguradora permanece no catálogo, fora da operação, até voltar a ter uma Modalidade Importada Ativa mapeada para ela. Grupo Inativo esconde suas Modalidades da operação sem apagá-las.
