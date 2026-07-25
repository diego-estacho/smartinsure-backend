# Jornada: Coberturas Adicionais

Cada RN é uma seção com o ID no título e os quatro blocos abaixo. O ID é `RN-NNN` numa **sequência única do catálogo** (não reinicia por jornada), estável e nunca reaproveitado. Linguagem de negócio, termos do [glossário](../glossario.md), sem path de código no corpo.

> Origem: especificação de produto "Coberturas Adicionais" (2026-07-23, AB#0003). Status: **proposta** — aguardando ratificação da PO. A Cobertura Adicional é o catálogo canônico do Smart de garantias complementares de uma Modalidade, curado pelo Administrador do Sistema e visto pelo corretor na cotação; a Cobertura Adicional Importada é a mesma garantia como cada Seguradora a expõe na OnPoint, trazida por importação e vinculada manualmente à canônica (irmã do Catálogo de Modalidades, [ADR-061](../../adr/061-modalidade-derivada-da-global-modality.md), RN-032..RN-039). Decisões abertas relacionadas: OPEN-16 (tipo de cálculo e edição manual, fora de escopo desta fase), OPEN-09 (credencial quando a Seguradora é habilitada por várias Corretoras) e OPEN-10 (cadência do agendamento).

## RN-040 — Cobertura Adicional é catálogo canônico curado

**Descrição.** A Cobertura Adicional é o item canônico do Smart — uma garantia complementar nomeada (ex.: Multa, Trabalhista e Previdenciária), sem dono de Seguradora — que o corretor vê na cotação. É mantida pelo Administrador do Sistema: criar, editar, ativar e inativar. Nunca é criada pela importação.

**Pré-condições.** Para escrita: usuário autenticado com Perfil Administrador do Sistema (mesmo padrão do catálogo de Seguradoras e de Modalidades, RN-011/RN-032).

**Critério de aceitação.** O Administrador cria, edita, ativa e inativa Cobertura Adicional. O nome é único no catálogo. Usuário sem o Perfil não escreve nem ativa/inativa. A importação nunca cria nem altera uma Cobertura Adicional canônica.

**Casos limite.** Nome duplicado é recusado. Cobertura Adicional nunca é excluída — sai de operação por Inativação (RN-044). O nome é normalizado (espaços das bordas removidos) antes da comparação de unicidade.

## RN-041 — Cobertura Adicional Importada reflete a fonte

**Descrição.** A Cobertura Adicional Importada é a garantia complementar como uma Seguradora a expõe na OnPoint, trazida pela importação por Modalidade Importada. Reflete a fonte: nome, identificador de origem, tipo de cálculo do valor segurado e a indicação de edição manual vêm da OnPoint e não são editados à mão. Toda Cobertura Adicional Importada nasce Ativa. Sua identidade é a combinação Modalidade Importada + nome.

**Pré-condições.** Importação bem-sucedida das Coberturas Adicionais de uma Modalidade Importada (RN-042).

**Critério de aceitação.** Uma cobertura trazida ainda não conhecida naquela Modalidade Importada (pelo nome normalizado) é criada como Cobertura Adicional Importada, Ativa; uma já conhecida é atualizada com os dados atuais e reativada, sem duplicar. O nome é normalizado antes de comparar ou gravar; cobertura sem nome é ignorada. Não há duas Coberturas Adicionais Importadas com o mesmo nome na mesma Modalidade Importada.

**Casos limite.** Nome só com espaços é tratado como sem nome (ignorado). A cobertura é vinculada à Modalidade Importada consultada; o ramo retornado (Público/Privado) confirma o ramo da Modalidade Importada — cobertura de ramo divergente ou sem Modalidade Importada a quem pertencer é descartada. O tipo de cálculo e a indicação de edição manual são preservados como recebidos, sem interpretação nesta fase (OPEN-16).

## RN-042 — Importação periódica e sob demanda pelo Administrador do Sistema

**Descrição.** Um processo periódico mantém o catálogo local de Coberturas Adicionais Importadas sincronizado com a OnPoint, percorrendo cada Modalidade Importada de Seguradora Ativa e habilitada. Além do agendamento automático, a importação pode ser disparada sob demanda pelo Administrador do Sistema. A cadência do agendamento é configurável, não fixa no código.

**Pré-condições.** Seguradora Ativa (RN-010) com Habilitação de Seguradora Ativa e Motor de Cálculo configurado (RN-022). Para o disparo sob demanda: usuário com Perfil Administrador do Sistema.

**Critério de aceitação.** A cada execução (agendada ou sob demanda), para cada Modalidade Importada processável de Seguradora Ativa e habilitada, a importação consulta a OnPoint e faz o upsert das Coberturas Adicionais Importadas por identidade Modalidade Importada + nome (RN-041). O disparo sob demanda por usuário sem o Perfil é recusado. A credencial e o endereço da OnPoint vêm da Habilitação, nunca fixos no código. A frequência do agendamento é ajustável por configuração.

**Casos limite.** Cadência de referência proposta: produção ao menos 1x/dia em horário de baixo pico; demais ambientes com maior frequência (OPEN-10). Seguradora habilitada por várias Corretoras é consultada por uma única credencial (OPEN-09). Modalidade Importada Inativa ou Ignorada não é consultada. Sem Motor de Cálculo resolvível, a Seguradora não é processada e fica registrada como não processada (RN-045).

## RN-043 — Vínculo manual da Importada à canônica; sem vínculo, fica pendente

**Descrição.** O vínculo entre uma Cobertura Adicional Importada e a Cobertura Adicional canônica é feito manualmente pelo Administrador do Sistema, na curadoria da Cobertura Adicional. Uma Cobertura Adicional Importada sem vínculo fica pendente de mapeamento e evidente na curadoria. O Administrador pode vincular, reatribuir o vínculo, e ignorar uma importada que não deva ser mapeada (ou reativar a ignorada).

**Pré-condições.** Cobertura Adicional Importada existente (RN-041) e, para vincular, uma Cobertura Adicional canônica existente (RN-040).

**Critério de aceitação.** O Administrador vincula uma Cobertura Adicional Importada a uma Cobertura Adicional canônica; várias importadas (de Seguradoras diferentes) podem apontar para a mesma canônica. Importada sem vínculo aparece como pendente de mapeamento. Ignorar retira a importada da lista de pendências sem vinculá-la; reativar desfaz o ignorar. Reatribuir troca a canônica vinculada. O vínculo manual é preservado nas reimportações — a importação nunca o altera.

**Casos limite.** Uma importada aponta para no máximo uma canônica. Ignorar não vincula nem exclui. Uma canônica inativada permanece vinculada às suas importadas (o vínculo não se desfaz pela inativação).

## RN-044 — Reconciliação preserva o catálogo importado

**Descrição.** Numa consulta bem-sucedida de uma Modalidade Importada, as Coberturas Adicionais Importadas que existiam localmente e não vieram mais na resposta da OnPoint são marcadas como Inativas. Nada é removido fisicamente — a inativação preserva o histórico e o vínculo manual, e é reversível: uma importação futura reativa a importada que reaparecer.

**Pré-condições.** Consulta bem-sucedida das Coberturas Adicionais de uma Modalidade Importada (RN-042).

**Critério de aceitação.** Após uma consulta bem-sucedida, importada ausente na resposta passa a Inativa sem ser apagada e mantendo seu vínculo (RN-043); importada que reaparece volta a Ativa (RN-041), mantendo identidade e vínculo. Uma importada Inativa permanece consultável. A desativação por reconciliação nunca ocorre para uma Modalidade Importada cuja consulta falhou (RN-045).

**Casos limite.** Modalidade Importada que deixou de existir: suas coberturas importadas passam a Inativas, sem exclusão. A Cobertura Adicional canônica nunca é desativada por reconciliação — sua situação é decisão de curadoria (RN-040).

## RN-045 — Resiliência e auditoria da importação

**Descrição.** A falha na consulta de uma Modalidade Importada não desativa suas Coberturas Adicionais Importadas nem interrompe as demais. Resposta com erro ou corpo nulo é tratada como falha daquela modalidade, registrada para auditoria; cada execução da importação é auditada com o resultado global e a lista de falhas por recurso.

**Pré-condições.** Execução da importação (RN-042).

**Critério de aceitação.** Resposta indicando erro ou corpo nulo de uma Modalidade Importada é registrada como falha (Seguradora, Modalidade Importada, ramo, status e mensagens) e não desativa suas coberturas importadas (RN-044); as demais Modalidades Importadas continuam sendo processadas. Cada execução registra sucesso/falha global e as falhas por recurso.

**Casos limite.** Todas as consultas falham: nada é desativado, o catálogo permanece intacto. Resposta em formato inesperado é tratada como falha da modalidade, nunca como catálogo vazio — não desativa nada por engano.

## RN-046 — Oferta pela Cobertura Adicional canônica

**Descrição.** O que o corretor vê na cotação é sempre a Cobertura Adicional canônica. Uma Cobertura Adicional é oferecida enquanto tiver ao menos uma Cobertura Adicional Importada Ativa vinculada a ela. A disponibilidade de uma Cobertura Adicional para uma Modalidade e Seguradora é derivada dos vínculos ativos, nunca digitada.

**Pré-condições.** Catálogo de Coberturas Adicionais canônicas e de Importadas alimentado, com vínculos (RN-040/RN-041/RN-043).

**Critério de aceitação.** Cobertura Adicional canônica Ativa com ao menos uma Importada Ativa vinculada é oferecida; sem nenhuma Importada Ativa vinculada, não é oferecida mas permanece no catálogo. Cobertura Adicional canônica Inativa não é oferecida. A disponibilidade por Modalidade e Seguradora é calculada a partir das Coberturas Adicionais Importadas Ativas vinculadas e das Modalidades Importadas a que pertencem.

**Casos limite.** A última Importada Ativa vinculada some: a canônica deixa de ser oferecida e permanece no catálogo. Coberturas Importadas pendentes (sem vínculo) ou Ignoradas não tornam nenhuma canônica ofertável.
