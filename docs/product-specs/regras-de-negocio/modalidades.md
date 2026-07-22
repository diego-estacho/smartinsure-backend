# Jornada: Catálogo de Modalidades

Cada RN é uma seção com o ID no título e os quatro blocos abaixo. O ID é `RN-NNN` numa **sequência única do catálogo** (não reinicia por jornada), estável e nunca reaproveitado. Linguagem de negócio, termos do [glossário](../glossario.md), sem path de código no corpo.

> Origem: especificação de produto "Catálogo de Modalidades" (2026-07-21, AB#0002), **revista em 2026-07-22** após alinhamento com o time. Status: **proposta** — aguardando ratificação da PO. Modelo revisto (ver [ADR-061](../../adr/061-modalidade-derivada-da-global-modality.md), supersede o ADR-060): a **Modalidade** (lado Smart) é derivada da **Modalidade Global** da OnPoint na importação (o vínculo vem pronto pelo id global) e também pode ser criada manualmente; **não há Grupo de Modalidade no lado Smart** e **não há tabela/etapa de mapeamento** — o vínculo Importada→Modalidade é direto (id global, com override manual). A automação "por semelhança" (nome) não existe. Decisões abertas relacionadas: OPEN-11 (disponibilidade PF/PJ).

## RN-029 — Modalidade é catálogo importado e curado

**Descrição.** A Modalidade é o item do catálogo do Smart que o corretor vê e compara. Nasce de duas formas: **derivada da Modalidade Global** da OnPoint na importação (criada automaticamente pelo id global, com o nome da fonte — RN-031/RN-032) ou **criada manualmente** pelo Administrador do Sistema. A escrita manual (criar, editar) e a ativação/inativação são restritas ao Administrador do Sistema. Não existe Grupo de Modalidade no lado Smart.

**Pré-condições.** Para escrita manual: usuário autenticado com Perfil Administrador do Sistema (mesmo padrão do catálogo de Seguradoras, RN-011).

**Critério de aceitação.** O Administrador cria, edita, ativa e inativa Modalidade. A importação cria/atualiza Modalidade automaticamente a partir da Modalidade Global (RN-032), sem intervenção humana. Uma Modalidade derivada é única por id da Modalidade Global; o nome é único no catálogo. Usuário sem o Perfil não escreve nem ativa/inativa.

**Casos limite.** Nome de Modalidade duplicado é recusado. Modalidade nunca é excluída — sai de operação por Inativação (RN-036). Uma Modalidade derivada e uma manual podem coexistir; reatribuir uma Modalidade Importada entre elas é ação de Fila (RN-034).

## RN-030 — Modalidade Importada reflete a fonte

**Descrição.** Cada Modalidade Importada reflete sempre a última importação bem-sucedida da Seguradora: nome de origem, ramo, grupo importado e parâmetros comerciais vêm da Seguradora e não são editados à mão na plataforma. Entre importações, a Modalidade Importada é reencontrada pelo identificador de origem, preservando sua identidade e seu vínculo com a Modalidade.

**Pré-condições.** Importação bem-sucedida de uma Seguradora (RN-031).

**Critério de aceitação.** Uma modalidade trazida ainda não conhecida (pelo identificador de origem) é criada como Modalidade Importada com os dados da Seguradora; já conhecida é atualizada com os dados atuais, mantendo identidade e vínculo. A plataforma não permite editar nome, ramo ou parâmetros comerciais da Modalidade Importada à mão. A data da última importação é registrada.

**Casos limite.** A Seguradora renomeia a modalidade: reconhecida pelo identificador de origem, atualiza o nome e mantém o vínculo — não vira item novo. Identificador de origem ausente na resposta é tratado como dado inválido: não cria item órfão.

## RN-031 — Importação periódica por Seguradora, motor resolvido pela Habilitação

**Descrição.** A importação de modalidades roda periodicamente e percorre as Seguradoras com Habilitação de Seguradora Ativa e Motor de Cálculo configurado, com uma consulta por Seguradora. O Motor de Cálculo e os parâmetros de conexão são resolvidos pela Habilitação (RN-023), nunca fixos no código. A importação alimenta o lado importado (RN-030) e, pela Modalidade Global, cria/atualiza a Modalidade e o vínculo (RN-032).

**Pré-condições.** Habilitação de Seguradora Ativa com Motor de Cálculo configurado (RN-022).

**Critério de aceitação.** A cada execução agendada, para cada Seguradora habilitada, a importação obtém o catálogo pelo Motor de Cálculo resolvido na Habilitação, cria/atualiza as Modalidades Importadas (RN-030) e as vincula às Modalidades pela Modalidade Global (RN-032). Seguradora sem Habilitação Ativa ou sem Motor de Cálculo configurado não é consultada.

**Casos limite.** A mesma Seguradora habilitada por várias Corretoras é consultada uma única vez, deduplicada por Seguradora — qual credencial é usada e se catálogos podem divergir entre Corretoras é decisão aberta (ver open-decisions). Seguradora Inativa no catálogo não é importada (RN-010). Sem Motor de Cálculo resolvível, a Seguradora não é importada e fica registrada como não processada.

## RN-032 — Vínculo automático pela Modalidade Global

**Descrição.** O vínculo entre uma Modalidade Importada e a Modalidade (do Smart) **vem pronto da fonte**: cada Modalidade Importada carrega o identificador da Modalidade Global da OnPoint, e a Modalidade do Smart é justamente essa Modalidade Global. Na importação, a Modalidade Importada é vinculada automaticamente à Modalidade daquele id global — a Modalidade é criada se ainda não existir. Não há mapeamento por nome nem etapa de confirmação.

**Pré-condições.** Modalidade Importada atualizada pela importação (RN-030), com o identificador da Modalidade Global conhecido.

**Critério de aceitação.** A Modalidade Importada recebe o vínculo com a Modalidade correspondente ao seu id de Modalidade Global (criada com o nome da Global se ainda não existir). Modalidades Importadas de Seguradoras diferentes com o mesmo id de Modalidade Global compartilham a mesma Modalidade — comparabilidade automática, sem ação humana. Um override manual anterior (RN-034) é **preservado**: a importação não sobrescreve um vínculo definido à mão.

**Casos limite.** Modalidade Importada sem identificador de Modalidade Global (exceção) fica sem vínculo automático e vai para a Fila de Revisão (RN-034). Uma Modalidade Global pode abranger mais de um ramo; o ramo é atributo da Modalidade Importada e não afeta o vínculo.

## RN-033 — Oferta e comparabilidade pela Modalidade

**Descrição.** O que o corretor vê é sempre a Modalidade. Uma Modalidade é oferecida enquanto tiver ao menos uma Modalidade Importada Ativa e não Ignorada vinculada a ela. Duas ofertas são "a mesma modalidade em Seguradoras diferentes" quando as Modalidades Importadas compartilham a mesma Modalidade (garantido pelo id da Modalidade Global). A disponibilidade da Modalidade por ramo é derivada, nunca digitada.

**Pré-condições.** Catálogo de Modalidades e de Modalidades Importadas alimentado (RN-031/RN-032).

**Critério de aceitação.** Modalidade sem nenhuma Modalidade Importada Ativa não Ignorada vinculada não aparece na operação, mas permanece no catálogo. Modalidade Importada Inativa ou Ignorada não é oferecida. A disponibilidade por ramo (ente público/privado) é calculada a partir das Modalidades Importadas Ativas vinculadas. A comparação entre Seguradoras usa a Modalidade como eixo.

**Casos limite.** A última Seguradora que oferecia a Modalidade sai: a Modalidade some da operação e permanece no catálogo. A disponibilidade por tipo de tomador (pessoa física/jurídica) é decisão aberta (OPEN-11). Os parâmetros comerciais de cada Seguradora afloram somente quando o corretor escolhe cotar naquela Seguradora.

## RN-034 — Fila de Revisão: exceções, override e ignorar

**Descrição.** Como o vínculo vem pronto da OnPoint (id da Modalidade Global), a Fila de Revisão — recorte evidenciado dentro do Mapa de Modalidades — trata apenas de **exceções e curadoria**: Modalidades Importadas sem identificador de Modalidade Global (sem vínculo automático) e a gestão de ignorar/reativar. O Administrador pode ainda reatribuir manualmente uma Modalidade Importada a outra Modalidade (override) e promover (criar uma Modalidade e vincular).

**Pré-condições.** Modalidade Importada sem vínculo automático, ou decisão do Administrador de reatribuir/ignorar.

**Critério de aceitação.** No Mapa, as exceções ficam evidenciadas. O Administrador vincula manualmente uma Modalidade Importada a uma Modalidade (registrado como manual, com quem e quando), e esse vínculo é preservado na reimportação (RN-032). Ignorar marca a Modalidade Importada como Ignorada — não é oferecida e não volta à Fila nas próximas importações, mas fica registrada. Reativar desfaz o Ignorar. Promover cria a Modalidade (RN-029) e vincula a pendência a ela.

**Casos limite.** Item ignorado permanece registrado e pode ser reavaliado. Reatribuir troca a Modalidade da Importada, com registro. A criação/uso de Modalidade manual respeita o nome único (RN-029).

## RN-035 — Resiliência da importação

**Descrição.** Falha de uma Seguradora numa importação não desativa nada dela nem impede as demais. Apenas a ausência de uma Modalidade Importada numa importação bem-sucedida daquela Seguradora a desativa.

**Pré-condições.** Execução da importação (RN-031).

**Critério de aceitação.** Erro, tempo excedido ou resposta inválida de uma Seguradora é registrado como falha daquela Seguradora, sem desativar suas Modalidades Importadas e sem afetar as demais Seguradoras. Numa importação bem-sucedida, as Modalidades Importadas que estavam Ativas e não vieram passam a Inativas. Reaparecendo numa importação futura, são reativadas, mantendo o vínculo anterior (inclusive um override manual).

**Casos limite.** Todas as Seguradoras falham: nada é desativado, o catálogo permanece intacto. Resposta em formato inesperado é tratada como falha, nunca como catálogo vazio — não desativa nada por engano.

## RN-036 — Preservação do catálogo

**Descrição.** Nada no catálogo de Modalidades é apagado. Modalidades, Modalidades Importadas e Grupos Importados saem de operação por Inativação (a Modalidade Importada também por Ignorar), mantendo histórico e permitindo retorno.

**Pré-condições.** Item existente no catálogo.

**Critério de aceitação.** Inativar um item o retira da operação sem removê-lo; um item Inativo permanece consultável e pode ser reativado. A importação usa Inativação (nunca exclusão) para o que deixou de vir (RN-035).

**Casos limite.** Modalidade sem nenhuma Seguradora permanece no catálogo, fora da operação, até voltar a ter uma Modalidade Importada Ativa vinculada. Modalidade Importada Ignorada permanece registrada e reavaliável (RN-034).
