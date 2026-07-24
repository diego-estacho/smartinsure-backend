# Decisões abertas

O que está listado aqui **não é implementável** até ser decidido. Agente que encontrar uma dependência aberta: pare e aponte a decisão, não invente. Decisão tomada vira ADR (ou atualização do glossário/RN) e a entrada sai daqui. A lista cresce conforme o trabalho real esbarra em bloqueio — não é um backlog planejado.

## OPEN-01 — Glossário canônico e máquina de estados
Dono: PO (gerente de projeto)
Bloqueia: qualquer código de domínio (nomes de entidades, rotas, telas, status)
Status: aberta
Contexto: a proposta está em [glossario.md](glossario.md). Já foram ratificados os status de Usuário e Corretora. Falta a PO ratificar os demais termos e enumerar os demais status do produto com as transições permitidas.

## OPEN-02 — Política de acesso de agentes de IA a dados (LGPD)
Dono: time + empresa (verificar se o grupo já tem política formalizada)
Bloqueia: uso de dados reais em fixtures, prompts e ambientes de teste
Status: aberta
Contexto: hoje não existe política formal. Até existir, vale o [SECURITY.md](../SECURITY.md): prod read-only pela credencial no servidor, dados sintéticos em teste.

## OPEN-03 — Vínculo entre Usuário e Corretora
Dono: PO (gerente de projeto)
Bloqueia: isolamento multi-tenant por corretora (query filters, ADR-035) e claims de corretora na identidade (ADR-014); restrição de quem pode criar usuário por perfil
Status: aberta
Contexto: a criação de Usuário (RN-001) nasce sem vínculo com Corretora e sem perfis, por decisão do negócio nesta fase. Falta decidir quando e como o Usuário passa a pertencer a uma Corretora e quais perfis restringem a criação — hoje qualquer usuário autenticado cria.

## OPEN-04 — Uso dos dados retornados pelo Birô
Dono: PO (gerente de projeto)
Bloqueia: qualquer efeito automático dos dados do Birô (preencher cadastro, bloquear ou alertar por situação cadastral) e os gatilhos de negócio que disparam a consulta
Status: aberta
Contexto: a consulta ao Birô existe como serviço reutilizável (RN-003, RN-004), mas o negócio ainda não definiu o que a plataforma faz com o retorno (situação cadastral, endereço, atividade econômica) nem em quais momentos da jornada a consulta dispara. Cada consulta tem custo por chamada e hoje não há reuso de respostas — se o volume crescer, a decisão de reuso/validade também é da PO.
Parcialmente resolvida em 2026-07-16 (RN-013..RN-016, jornada Cadastro de Pessoas): a busca por CNPJ não cadastrado dispara a consulta e o retorno preenche o cadastro da Pessoa Jurídica (nome, fantasia, natureza jurídica, endereço principal), importado uma única vez. Parcialmente resolvida em 2026-07-17 (RN-019, jornada Corretoras): a criação de Corretora por CNPJ usa a busca de Pessoa e importa a Pessoa jurídica pelo Birô quando necessário. Segue em aberto: uso da situação cadastral (bloquear/alertar), demais gatilhos da jornada e reuso/validade de respostas.

## OPEN-05 — Bloqueio por tentativas repetidas de login
Dono: PO (gerente de projeto)
Bloqueia: qualquer mecanismo de bloqueio/atraso após falhas de autenticação (RN-005 nasce sem ele)
Status: aberta
Contexto: a RN-005 recusa credenciais incorretas com mensagem genérica, mas não define limite de tentativas nem tempo de bloqueio. Falta a PO decidir se haverá bloqueio, com quais limites, e se o mecanismo nativo do provedor de identidade atende.

## OPEN-06 — Primeiro acesso via convite (substitui a senha inicial padrão)
Dono: PO (gerente de projeto) — endereçada pela futura funcionalidade de convite
Bloqueia: revisão de RN-001 (fim da senha inicial padrão) e de RN-002 (ativação passa a ocorrer pelo link de convite, sem autenticação prévia)
Status: aberta
Contexto: decidido em 2026-07-16 que o Usuário Pendente não se autentica (RN-005); a primeira senha será definida pelo próprio Usuário através de link de convite enviado por e-mail — uso único, com prazo de validade (proposta: 7 dias) e reenviável enquanto o Usuário for Pendente. O envio de e-mail ainda não existe e será implementado na funcionalidade de convite, quando RN-001 e RN-002 serão revisadas. Decidido em 2026-07-16: o login recusa o Usuário Pendente sempre (RN-005 literal) — usuário novo permanece sem acesso até a funcionalidade de convite existir; a ativação (RN-002) fica inoperante nesse intervalo por decisão consciente do negócio.

## OPEN-07 — Comportamento do cotar Ofertas
Dono: PO (gerente de projeto)
Bloqueia: a funcionalidade de cotar Ofertas (disparo das Cotações a partir de uma Oferta)
Status: aberta
Contexto: decidido em 2026-07-19 que esta fase entrega apenas a infraestrutura do Motor de Cálculo (RN-022..RN-024) — Habilitação de Seguradora e resolução do motor por configuração, com PlugV2 como único motor. A direção indicada pelo negócio é que cotar dispare para TODAS as Seguradoras habilitadas da Corretora (uma Cotação por Seguradora, conforme glossário), mas a demanda do cotar ainda não foi especificada; escopo, disparo (todas vs. uma escolhida), momento e experiência do corretor serão definidos na demanda própria. Também segue aberto quem pode gerenciar a Habilitação de Seguradora — nesta fase qualquer usuário autenticado (mesma pendência de perfis da OPEN-03).

## OPEN-08 — Validade do limite e funcionalidades complementares da Consulta de Crédito
Dono: PO (gerente de projeto)
Bloqueia: exibição da validade do limite na Consulta de Crédito (RN-029); Registro Manual de Limite; Solicitação de Análise de Crédito pela assessoria
Status: aberta
Contexto: decidido em 2026-07-20 que esta fase entrega apenas a consulta online de Limites de Crédito com histórico (RN-029..RN-031). Parcialmente resolvida em 2026-07-21: o retorno real do motor traz limite revisado e disponível por modalidade — o limite utilizado passou a ser derivado (revisado − disponível) e incluído na RN-029; os grupos de modalidade são dinâmicos, informados pela Seguradora. Segue em aberto: a validade do limite não tem fonte no retorno do motor (a tela apresenta como ausente) — decidir fonte ou remoção. O registro manual de limite (informado por portal, telefone ou e-mail da seguradora) e a solicitação de análise pela assessoria ficaram fora desta entrega e serão especificados em demanda própria. A lista de "tomadores pesquisados recentemente" foi decidida como conveniência de tela, sem persistência — não gera RN. Pendência adicional (2026-07-20, apontada em code review): a RN-029 não define a fórmula do "limite consolidado" do resumo — a implementação usa a soma, por Seguradora disponível, do maior limite entre as modalidades; confirmar a fórmula com a PO (documentada no contrato do endpoint).

## OPEN-09 — Credencial e divergência de catálogo quando a Seguradora tem várias Corretoras habilitadas
Dono: PO (gerente de projeto)
Bloqueia: a regra de qual credencial (PlugKey) usar na importação de uma Seguradora habilitada por mais de uma Corretora, e o tratamento caso o catálogo retornado divirja entre Corretoras
Status: aberta
Contexto: levantado em 2026-07-21 (jornada Catálogo de Modalidades, AB#0002). A Modalidade Importada é da Seguradora, não da Corretora (o `BrokerCnpj`/PlugKey é só credencial de busca), então a importação deduplica por Seguradora e faz uma chamada por Seguradora (RN-034). Falta a PO decidir qual credencial usar quando várias Corretoras habilitam a mesma Seguradora e o que fazer se o retorno divergir entre elas (hoje assume-se catálogo único por Seguradora). Reusada por analogia em 2026-07-23 pela jornada Coberturas Adicionais (AB#0003, RN-043): a importação de coberturas herda a mesma dedução por Seguradora e o mesmo tratamento de credencial.

## OPEN-10 — Cadência do agendamento da importação de modalidades
Dono: PO (gerente de projeto)
Bloqueia: nada crítico (há default proposto); ajusta a frequência do job
Status: aberta
Contexto: levantado em 2026-07-21 (jornada Catálogo de Modalidades, AB#0002). A importação roda periodicamente por agendamento (RN-034). Proposta de default: diária, em horário de baixo pico, com a cadência configurável (não fixa no código). Falta a PO confirmar se há requisito de frequência específico (ex.: mais de uma vez ao dia, ou alinhado a janela da Seguradora). Reusada por analogia em 2026-07-23 pela jornada Coberturas Adicionais (AB#0003, RN-044): a cadência do job de coberturas também é configurável (não fixa no código); default proposto na spec = produção 1x/dia às 05:00 e demais ambientes a cada 30min, pendente da mesma confirmação da PO. Atualização 2026-07-23 (jornada Tags e Cláusulas, AB#0004): a importação de Tags e Cláusulas roda no mesmo ciclo de catálogo (RN-047..RN-049), então a cadência é única (mesmo default: prod 05:00 / demais 30min); segue aberto para a PO confirmar os valores definitivos e se há janela específica por Seguradora.

## OPEN-11 — Disponibilidade derivada por tipo de tomador (PF/PJ)
Dono: PO (gerente de projeto)
Bloqueia: a parte "pessoa física / jurídica" da disponibilidade derivada da Modalidade (RN-036)
Status: aberta
Contexto: levantado em 2026-07-22 (fatia 3, Mapa de Modalidades). A disponibilidade **por ramo** (ente público/privado) é derivada com segurança do `Branch` das Modalidades Importadas ativas confirmadas. Já a disponibilidade **PF/PJ** dependeria de interpretar os flags do PlugV2 (`IgnoreBranchWhenInsuredIsPF`, `IgnoreBranchWhenInsuredIsPrivate`), cuja semântica exata (o que "ignorar ramo quando o segurado é PF" significa para "disponível para PF") não está definida. Não foi implementada para não inventar regra; falta a PO definir a semântica (e, se preciso, tipar público-alvo na Modalidade Importada). Até lá, o Mapa mostra a disponibilidade por ramo.

## OPEN-12 — Granularidade da Modalidade vs. Global Modality do motor (mapeamento por identificador)
Dono: PO (gerente de projeto)
Bloqueia: a semântica do mapeamento automático "por identificador do motor" (RN-035) quando a Global Modality do motor é mais grossa que a Modalidade desejada
Status: **resolvida** em 2026-07-22 (ADR-061) — opção (A)
Contexto: levantado em 2026-07-22. O PlugV2/OnPoint agrupa várias ofertas sob uma mesma **Global Modality** (identificador do motor). Ex.: id 31 = "Judicial" reúne, só na Essor, 10 origens distintas — "Judicial - Cível", "Judicial - Execução Fiscal", várias "PGE …". O modelo antigo (ADR-060) herdava o mapeamento por identificador a partir de uma semente confirmada, o que levava a lumping semanticamente questionável.
Resolução: o time decidiu pela **opção (A)** — a **Modalidade equivale à Modalidade Global** da OnPoint (a fonte é a autoridade da granularidade). O ADR-061 formaliza: a Modalidade é derivada da Modalidade Global (find-or-create por id global), o vínculo é intrínseco, e não há semente/confirmação manual para propagar. Assim "Judicial" é uma única Modalidade por definição, e o problema de granularidade deixa de existir. Correção de dados legados (mapeamentos criados sob o ADR-060) será feita no retrabalho da implementação para o modelo do ADR-061.

## OPEN-13 — Nome único da Modalidade vs. identidade por id de Modalidade Global
Dono: PO (gerente de projeto)
Bloqueia: importação de uma Seguradora cujo id de Modalidade Global traz um nome que colide com uma Modalidade manual já existente
Status: aberta
Contexto: levantado em 2026-07-22 (retrabalho ADR-061). A Modalidade tem identidade pelo id de Modalidade Global (derivada) e o nome é único no catálogo. Uma Modalidade **criada manualmente** com um nome que depois chega como nome de uma Modalidade Global (find-or-create por id global) **bloqueia** o create daquela derivada — a importação daquela Seguradora falha por conflito de nome. Ocorreu em dev com a "Licitante" manual legada (limpa como dado de dev). Falta a PO decidir o comportamento: (a) a derivada por id global tem precedência e "adota"/renomeia a manual homônima; (b) a manual é reatribuída à derivada; (c) permite nomes duplicados quando um lado é derivado; ou (d) alerta e deixa o Administrador resolver na Fila. Até decidir, evita-se criar Modalidade manual com nome de uma Global existente.

## OPEN-14 — Exibição da Fila de Revisão (feature-flag)
Dono: PO (gerente de projeto)
Bloqueia: a exibição da Fila de Revisão no Mapa de Modalidades (front)
Status: aberta
Contexto: levantado em 2026-07-22. Como o vínculo Modalidade Importada → Modalidade vem pronto pela Modalidade Global (ADR-061), no fluxo normal não há pendências de curadoria (toda importada tem id global). Por isso a **Fila de Revisão foi ocultada no front por feature-flag** (`NUXT_PUBLIC_MODALITY_REVIEW_QUEUE`, default `false`) — **a implementação permanece intacta e testada** (Reatribuir/Ignorar/Reativar no composable/BFF/endpoints + dialogs). Falta a PO decidir se/quando reexibir: reativar a flag (`true`) quando o **cadastro manual de Modalidades** e/ou o **tratamento de exceções** (importadas sem id de Modalidade Global) for decidido e fizer sentido operacional. Relacionada a OPEN-13. Enquanto isso, a Fila não aparece; o Mapa mostra só a matriz Seguradoras × Modalidades.

## OPEN-15 — Mapeamento automático de modalidade "por semelhança"
Dono: PO (gerente de projeto)
Bloqueia: —
Status: **resolvida** em 2026-07-22 (ADR-061)
Contexto: levantado em 2026-07-21 (originalmente OPEN-08; renumerado para OPEN-15 na integração com a jornada Consulta de Crédito, que passou a ocupar o OPEN-08 no tronco). **Encerrada** com a revisão do modelo (ADR-061): o vínculo Modalidade Importada → Modalidade passa a ser intrínseco, pelo id da Modalidade Global da OnPoint — não há aproximação por nome/descrição em nenhuma forma. A "semelhança" deixa de existir no domínio de modalidades.

## OPEN-16 — Semântica do tipo de cálculo do valor segurado e uso da edição manual da Cobertura Adicional
Dono: PO (gerente de projeto)
Bloqueia: qualquer efeito de negócio do `InsuredAmountCalculationType` e do `AllowManualEdit` da Cobertura Adicional (precificação/cálculo do valor segurado na cotação, permitir/bloquear edição manual do valor)
Status: aberta
Contexto: levantado em 2026-07-23 (jornada Coberturas Adicionais, AB#0003). A importação traz da OnPoint, por cobertura, o tipo de cálculo do valor segurado (inteiro) e a indicação de edição manual permitida (booleano). Nesta fase esses dois campos são **importados e preservados como recebidos, sem interpretação** (RN-040) — a precificação da cobertura na cotação e a edição de valores pelo corretor estão fora de escopo da spec. Falta a PO definir a semântica dos valores do tipo de cálculo (enumerar por nome estável, conforme regra do glossário) e o comportamento da edição manual, quando a jornada de cotação com coberturas for especificada.
