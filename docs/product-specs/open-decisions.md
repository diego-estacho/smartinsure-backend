# Decisões abertas

O que está listado aqui **não é implementável** até ser decidido. Agente que encontrar uma dependência aberta: pare e aponte a decisão, não invente. Decisão tomada vira ADR (ou atualização do glossário/RN) e a entrada sai daqui. A lista cresce conforme o trabalho real esbarra em bloqueio — não é um backlog planejado.

## OPEN-01 — Glossário canônico e máquina de estados
Dono: PO (gerente de projeto)
Bloqueia: qualquer código de domínio (nomes de entidades, rotas, telas, status)
Status: aberta
Contexto: a proposta está em [glossario.md](glossario.md). Já foram ratificados os status de Usuário e Corretora. Falta a PO ratificar os demais termos e enumerar os demais status do produto com as transições permitidas.
Parcialmente resolvida em 2026-07-24 (jornada Grupo de Cotação): por decisão do dono, o agregado antes chamado **Oferta** (`Offer`) foi renomeado para **Grupo de Cotação** (`QuotationGroup`) e cada retorno de Seguradora, antes `Quote`, para **Cotação** (`Quotation`); a UI mantém "oferta" como rótulo provisório. Foi enumerado o único status desta fase — **Rascunho** (`Draft`) do Grupo de Cotação (RN-050, RN-051). Seguem em aberto os demais termos/estados e os estados posteriores do Grupo de Cotação (cotação obtida, proposta aceita, apólice emitida — OPEN-07).

## OPEN-02 — Política de acesso de agentes de IA a dados (LGPD)
Dono: time + empresa (verificar se o grupo já tem política formalizada)
Bloqueia: uso de dados reais em fixtures, prompts e ambientes de teste
Status: aberta
Contexto: hoje não existe política formal. Até existir, vale o [SECURITY.md](../SECURITY.md): prod read-only pela credencial no servidor, dados sintéticos em teste.

## OPEN-03 — Vínculo entre Usuário e Corretora
Dono: PO (gerente de projeto)
Bloqueia: isolamento multi-tenant por corretora (query filters, ADR-035) e claims de corretora na identidade (ADR-014); restrição de quem pode criar usuário por perfil
Status: parcialmente resolvida em 2026-07-23
Contexto: a criação de Usuário (RN-001) nasce sem vínculo com Corretora e sem perfis, por decisão do negócio nesta fase. Falta decidir quando e como o Usuário passa a pertencer a uma Corretora e quais perfis restringem a criação — hoje qualquer usuário autenticado cria.
Parcialmente resolvida em 2026-07-23 (RN-062..RN-076, jornada Perfis e Permissões): definidos o vínculo Usuário↔Corretora (N por Usuário, cada um portando o Perfil naquela Corretora), a Corretora ativa como gate das Permissões efetivas, o modelo Perfil×Escopo×Permissão, a hierarquia de criação (Admin→Corretor Administrador; Corretor Administrador→Tomador Administrador/Corretor; Tomador Administrador→Tomador; comuns por Permissão) e as verificações de nomeação para o Tomador Administrador. Depende da ratificação da PO dos termos novos e da nova cardinalidade de Perfil no glossário. Segue aberto: o isolamento multi-tenant técnico (query filters ADR-035, claims ADR-014) continua sob esta pendência até implementado.

## OPEN-04 — Uso dos dados retornados pelo Birô
Dono: PO (gerente de projeto)
Bloqueia: qualquer efeito automático dos dados do Birô (preencher cadastro, bloquear ou alertar por situação cadastral) e os gatilhos de negócio que disparam a consulta
Status: aberta
Contexto: a consulta ao Birô existe como serviço reutilizável (RN-003, RN-004), mas o negócio ainda não definiu o que a plataforma faz com o retorno (situação cadastral, endereço, atividade econômica) nem em quais momentos da jornada a consulta dispara. Cada consulta tem custo por chamada e hoje não há reuso de respostas — se o volume crescer, a decisão de reuso/validade também é da PO.
Parcialmente resolvida em 2026-07-16 (RN-013..RN-016, jornada Cadastro de Pessoas): a busca por CNPJ não cadastrado dispara a consulta e o retorno preenche o cadastro da Pessoa Jurídica (nome, fantasia, natureza jurídica, endereço principal), importado uma única vez. Parcialmente resolvida em 2026-07-17 (RN-019, jornada Corretoras): a criação de Corretora por CNPJ usa a busca de Pessoa e importa a Pessoa jurídica pelo Birô quando necessário. Parcialmente resolvida em 2026-07-25 (RN-052, redesign do CRUD de Corretoras, exec-plan 0009): a consulta de CNPJ no cadastro de Corretora passa a ser **somente leitura** (não grava Pessoa, papel nem endereço) e a Corretora só é criada na confirmação (RN-019 revisada) — decidido apenas o gatilho de preview do cadastro de Corretora, sem efeito sobre situação cadastral; ratificado por Diego Estácho no lugar da PO (registrar confirmação da PO). Parcialmente resolvida em 2026-07-27 (RN-052 revisada): decidido o reuso de respostas do Birô no cadastro de Corretora — a consulta persiste a Pessoa jurídica sem o Papel de corretor para reuso, com validade de 90 dias (após esse prazo, reconsulta o Birô apenas para exibição, sem alterar a Pessoa jurídica armazenada — import-once) e sem limpeza automática das Pessoas nunca confirmadas; ratificado por Diego Estácho no lugar da PO (registrar confirmação da PO). Segue em aberto: uso da situação cadastral (bloquear/alertar) e demais gatilhos da jornada.

## OPEN-05 — Bloqueio por tentativas repetidas de login
Dono: PO (gerente de projeto)
Bloqueia: qualquer mecanismo de bloqueio/atraso após falhas de autenticação (RN-005 nasce sem ele)
Status: aberta
Contexto: a RN-005 recusa credenciais incorretas com mensagem genérica, mas não define limite de tentativas nem tempo de bloqueio. Falta a PO decidir se haverá bloqueio, com quais limites, e se o mecanismo nativo do provedor de identidade atende.

## OPEN-06 — Primeiro acesso via convite (substitui a senha inicial padrão)
Dono: PO (gerente de projeto) — endereçada pela futura funcionalidade de convite
Bloqueia: revisão de RN-001 (fim da senha inicial padrão) e de RN-002 (ativação passa a ocorrer pelo link de convite, sem autenticação prévia)
Status: parcialmente resolvida em 2026-07-23
Contexto: decidido em 2026-07-16 que o Usuário Pendente não se autentica (RN-005); a primeira senha será definida pelo próprio Usuário através de link de convite enviado por e-mail — uso único, com prazo de validade (proposta: 7 dias) e reenviável enquanto o Usuário for Pendente. O envio de e-mail ainda não existe e será implementado na funcionalidade de convite, quando RN-001 e RN-002 serão revisadas. Decidido em 2026-07-16: o login recusa o Usuário Pendente sempre (RN-005 literal) — usuário novo permanece sem acesso até a funcionalidade de convite existir; a ativação (RN-002) fica inoperante nesse intervalo por decisão consciente do negócio.
Endereçada em 2026-07-23 (RN-065, jornada Perfis e Permissões): o mecanismo de Convite (link de uso único, validade, reenvio) e o primeiro acesso com senha própria foram especificados, revisando RN-001 (fim da senha inicial padrão) e RN-002 (ativação pelo link). RN-001 e RN-002 foram reescritas como PROPOSTA em 2026-07-23 (bloco de revisão no topo de cada uma em usuarios.md), refletindo o convite. Correção 2026-07-23: o transporte de e-mail JÁ EXISTE (`IMailService`/`MailKitMailService`, ADR-048) — a antiga afirmação de "serviço de e-mail inexistente" está superada. Restante pendente da PO: confirmar o prazo de validade do link (proposta 7 dias) e ratificar a revisão de RN-001/RN-002. O mecanismo de convite (geração/validação do link de uso único e o caso de uso de envio) é implementação, não bloqueio.

## OPEN-07 — Comportamento do cotar Grupos de Cotação
Dono: PO (gerente de projeto)
Bloqueia: a funcionalidade de cotar (disparo das Cotações a partir de um Grupo de Cotação) e a emissão
Status: **etapa de cotações ratificada em 2026-07-28** (por Diego Estácho no lugar da PO — registrar confirmação da PO); a **emissão** segue fora de escopo (demanda própria)
Contexto: decidido em 2026-07-19 que esta fase entrega apenas a infraestrutura do Motor de Cálculo (RN-022..RN-024) — Habilitação de Seguradora e resolução do motor por configuração, com PlugV2 como único motor. A direção indicada pelo negócio é que cotar dispare para TODAS as Seguradoras habilitadas da Corretora (uma Cotação por Seguradora, conforme glossário), mas a demanda do cotar ainda não foi especificada; escopo, disparo (todas vs. uma escolhida), momento e experiência do corretor serão definidos na demanda própria. Também segue aberto quem pode gerenciar a Habilitação de Seguradora — nesta fase qualquer usuário autenticado (mesma pendência de perfis da OPEN-03).
Parcialmente resolvida em 2026-07-24 (jornada Grupo de Cotação): a **persistência do Grupo de Cotação em Rascunho** — o pedido que o corretor monta no wizard até concluir a etapa de risco (RN-050 criação, RN-051 atualização em Rascunho) — entrou em escopo e é do backend. Seguem mockados no front e fora de escopo desta fase: o disparo das Cotações às Seguradoras (etapa de cotações), a invalidação/recálculo por mudança de dados, e a emissão (etapa de emissão).
Parcialmente resolvida em 2026-07-27 (jornada Cotação): a **etapa de cotações** entrou em escopo — solicitar Cotações às Seguradoras a partir do Grupo (escopo *todas* × *escolhidas*, RN-056), obter e persistir cada Cotação por Seguradora com tolerância a falha isolada (RN-057), classificar o resultado de forma estável com esteira e motivos (RN-058), selecionar uma Cotação seguível (RN-059), recalcular/invalidar por qualquer mudança de dado com confirmação bloqueante (RN-060) e a validade da Cotação (RN-061). Direção do disparo confirmada: *todas as habilitadas* é o padrão recomendado, com *escolhidas* como alternativa; o disparo inclui Seguradoras que não ofertam a Modalidade (retornam indisponível com motivo) por decisão de transparência ao corretor. Seguem **fora de escopo** (demanda própria): o encaminhamento da Cotação escolhida (followup da análise de subscrição, aceite como Proposta e emissão), a Página de Listagem de Cotações, e quais Perfis podem solicitar cotação/gerenciar Habilitação (OPEN-03). A confirmar com a PO: (a) a classificação de **tomador nomeado** na cotação — indisponibilidade (beco sem saída) ou situação acionável (a Corretora pode se nomear e seguir)?; (b) o prazo de validade da Cotação (~15 dias) e sua fonte no contrato do fornecedor. Refinado com autonomia do time; **aguardando ratificação da PO** (a esta ratificação cabe fechar a entrada, junto com a emissão).
Ratificada em 2026-07-28 (etapa de cotações / Passo 4, por Diego Estácho no lugar da PO — registrar confirmação da PO): o desenho foi refinado por entrevista (`.grill/passo-4-cotacao.md`) e aprovado. Resoluções dos dois `[A CONFIRMAR]`: (a) **tomador nomeado** = indisponibilidade **informativa** (a nomeação/transferência é evolução futura, não acionável nesta fase); (b) **validade da Cotação por tempo** = **deferida** — o provedor não retorna validade; existe um cancelamento por inatividade (~15 dias) do lado do provedor, a ser **espelhado quando o cancelamento for implementado** (demanda própria); nesta fase a única invalidação é por mudança de dado (RN-060). Refinamentos ratificados: o fan-out cota **todas** as Seguradoras do escopo (o veredito automática×subscrição só existe cotando — cotar cria proposta); **sem nova tentativa automática** e **timeout alinhado ao teto do provedor** (RN-057, por não-idempotência); o modo *escolhidas* mostra as não-selecionadas como indisponíveis **locais** (RN-056); a **minuta** (Tags + Cláusulas particulares) e o **envio dos termos** entram no escopo do Passo 4 (RN-062, RN-063). Seguem fora de escopo: emissão, followup/Página de detalhes da Cotação, e o **cancelamento** das Cotações (irmãs na emissão, saída, expiração) — demanda própria. Nova dependência levantada: **OPEN-17** (cláusula particular que altera o veredito).

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

## OPEN-17 — Nomes técnicos dos Perfis fixos (colisão com Papel da Pessoa)
Dono: PO (gerente de projeto) + time (ADR-058, dono do vocabulário)
Bloqueia: nada — resolvida
Status: **decidida pelo dono do produto em 2026-07-29** — Perfil Corretor = `BrokerageUser`; Perfil Tomador = `PolicyHolderUser` (simetria com `BrokerageAdministrator`/`PolicyHolderAdministrator`, sem colidir com `EPersonRole.Broker`/`PolicyHolder`). Os nomes já ratificados por essa decisão são os cinco Perfis fixos: `SystemAdministrator`, `BrokerageAdministrator`, `PolicyHolderAdministrator`, `BrokerageUser`, `PolicyHolderUser`. Ratificação formal da PO segue pendente; se ela mudar um nome, é renomear seed + constante (o modelo referencia Perfil por id, não por nome).
Contexto: os Perfis fixos "Corretor" e "Tomador" (papéis de acesso do Usuário) colidem em nome com o Papel da Pessoa Corretor (`Broker`) e Tomador (`PolicyHolder`), que já existem no glossário e são conceitos distintos (o papel da Pessoa não é o Perfil do Usuário). Precisa a PO/time decidir os nomes técnicos 1:1 desses dois Perfis fixos (ex.: `BrokerProfile`/`PolicyHolderProfile` ou outro) antes de qualquer código, sob a regra do ADR-058. Corretor Administrador e Tomador Administrador já têm proposta de nome técnico no glossário (`BrokerageAdministrator`/`PolicyHolderAdministrator`), também sujeita a ratificação.

## OPEN-18 — Remoção de Permissão essencial à própria administração
Dono: PO (gerente de projeto)
Bloqueia: o comportamento da edição de Perfil fixo (RN-073) quando a Permissão removida é a que sustenta a administração (ex.: gerenciar Usuários no Corretor Administrador)
Status: aberta
Contexto: a RN-073 permite ao Administrador do Sistema editar as Permissões dos Perfis fixos com efeito global. Não foi decidido o que a plataforma faz se ele remover a própria Permissão que sustenta a administração de um Escopo — se bloqueia (como a RN-076 faz ao impedir Escopo sem administrador), se avisa, ou se apenas registra. Sem precedente ratificado, a RN-073 declara o efeito como não definido nesta fase.

## OPEN-19 — Mecânica do Escopo ativo (Corretora/Tomador ativo) e escopo padrão no primeiro acesso
Dono: arquitetura (ADR) + PO (comportamento no primeiro acesso)
Bloqueia: a resolução do Escopo ativo em tempo de request na RN-064 (permissões efetivas por Corretora/Tomador ativo) e, por consequência, os query filters multi-tenant por Corretora ativa (ADR-035). Não bloqueia as tabelas/entidades de vínculo (Usuário↔Corretora/Tomador), que são o N:N do glossário e podem nascer antes.
Status: **mecânica decidida pelo dono do produto em 2026-07-29** — segue o ADR-065 (Escopo ativo como claim do acesso; a troca reemite o acesso validando o vínculo). Escopo padrão no primeiro acesso, também decidido: **vínculo único vira ativo automaticamente** (RN-064 já diz isso nos casos limite) e, com mais de um vínculo, a seleção é oferecida antes de operar no escopo. Continua pendente **apenas a ratificação formal da mecânica pela arquitetura** (dono do ADR-065): se ela recusar o claim, muda o transporte do Escopo ativo, não as regras que o consomem.
Direção proposta (2026-07-23): a mecânica candidata é Escopo ativo carregado como claim do acesso (ADR-065, status proposto — aguardando ratificação do dono de arquitetura), coerente com o ADR-014; troca reemite o acesso validando o vínculo; sem tabela de sessão nova. Candidatos avaliados: (a) claim no JWT (escolhido — stateless, troca reemite o token); (b) sessão/estado no servidor; (c) header por request; (d) híbrido. Segue ABERTO: (a) ratificação da mecânica pela arquitetura (ADR-065); (b) dono PO/UX — qual o Escopo ativo padrão no primeiro acesso quando o Usuário tem mais de um vínculo (a única vira ativa automaticamente? seleção obrigatória antes de operar?). A fatia 1b (carregamento/troca da claim) fica pendente de (a) e (b); a fatia 1a (vínculos) não depende delas.

## OPEN-20 — Autoridade de inativação por escopo e Usuário multi-Corretora (RN-076)
Dono: PO (gerente de projeto)
Bloqueia: a inativação/reativação de Usuário pelos atores de escopo da RN-076 (Corretor Administrador, Tomador Administrador, usuário comum com permissão) — a fatia entregue faz apenas a do Administrador do Sistema.
Status: aberta
Contexto: a inativação torna o Usuário Inativo globalmente ("Usuário Inativo não acessa a plataforma"), mas um Usuário pode estar vinculado a várias Corretoras/Tomadores (RN-064). A RN-076 diz que o Corretor Administrador inativa "usuários das suas corretoras" — não está resolvido se um CA pode inativar GLOBALMENTE um Usuário que também pertence a outra Corretora (de outro CA), ou se a ação deveria ser apenas a remoção do vínculo naquele escopo. Também depende do enforcement por permissão (RN-063, adiado) para o caso do usuário comum. Enquanto aberto, só o Administrador do Sistema inativa/reativa (global, sem ambiguidade). Decidir a semântica (global vs por escopo) antes de estender aos demais atores.
Inclui também o **guard do lado do alvo** da RN-076 ("inativação que deixaria uma Corretora ou Tomador sem nenhum administrador é recusada"): a fatia entregue implementa apenas o guard de Escopo System (não deixar a plataforma sem Administrador do Sistema); a proteção equivalente para Corretora (último Corretor Administrador) e Tomador (último Tomador Administrador) fica com esta decisão — depende de contagem de administradores ativos por Escopo, que só faz sentido junto com a semântica global-vs-escopo. Limitação atual consciente: o Administrador do Sistema pode inativar o último Corretor/Tomador Administrador de um Escopo.

## OPEN-21 — Cláusula particular que impede emissão automática (altera o veredito da Cotação)
Dono: PO (gerente de projeto)
Bloqueia: a **re-avaliação** do veredito da Cotação (automática → subscrição) quando o corretor marca uma cláusula particular não-automática. **NÃO bloqueia** o fluxo do Passo 4 — a minuta é capturada e enviada normalmente (RN-062/063).
Status: aberta
Contexto: levantado em 2026-07-28 (jornada Cotação, Passo 4; antes OPEN-17, renumerado para OPEN-21 por colisão com os Perfis fixos na integração com a main). O gateway OnPoint/Plug V2 tem regra — **documentada** e replicada em ~11 plugins de Seguradora — em que uma cláusula particular com `AllowAutomaticIssue=false` (e não-fixa) encaminha a proposta para a esteira de **subscrição** em vez de emitir automaticamente (evidência levantada e conferida no código legado: `evidencia-clausula-particular-subscricao.md`/`.pdf`, área de trabalho do dev). Falta a PO confirmar se, no produto novo, marcar tal cláusula deve **re-avaliar** o veredito da Cotação (podendo virar subscrição) ou se isso é tratado apenas na emissão. Até decidir, o Passo 4 **não re-avalia** o veredito por causa de cláusula — captura a minuta e mantém o resultado da cotação; tags/texto da minuta não afetam o veredito.

## OPEN-90 — Efeito da Filial fora do Grupo de Cotação
Dono: PO (gerente de projeto)
Bloqueia: qual CNPJ (matriz ou Filial) é enviado à Seguradora ao cotar; Consulta de Crédito por Filial; Nomeação de Tomador por estabelecimento; remoção/desvínculo de Filial
Status: aberta
Contexto: levantado em 2026-07-27 (jornada Tomadores/Grupo de Cotação, AB#0005). A Filial passa a existir como cadastro vinculado à matriz (RN-052) e a ser registrada como estabelecimento cotado do Grupo de Cotação (RN-053). Por decisão do negócio, nesta entrega o efeito da Filial se limita a **registro e exibição**: a Consulta de Crédito (RN-029) continua consultando o CNPJ informado na tela, e a Nomeação de Tomador (RN-027, RN-028) continua valendo por Tomador (matriz), não por estabelecimento.
Parcialmente resolvida em 2026-07-28 (decisão do dono), itens (a) e (b) encerrados — são coisas separadas e a divisão é esta:
- **(b) Limite de Crédito e taxa são sempre da matriz.** A Seguradora não consulta limite pelo CNPJ da Filial. A Consulta de Crédito (RN-029) **não** passa a aceitar Filial, e a Filial marcada no Grupo de Cotação não altera limites nem taxas.
- **(a) Ao cotar, o CNPJ enviado à Seguradora é o da Filial marcada, e a apólice é emitida para o CNPJ da Filial.** Que a Seguradora avalie o risco com base na matriz é **funcionamento interno dela**, não comportamento da plataforma — o Smart envia o estabelecimento cotado. Sem Filial marcada, envia o da matriz (RN-053).
A implementação de (a) chega quando as etapas de cotação e emissão saírem do mock (OPEN-07); a regra, porém, já está definida e não depende mais de decisão.
Segue em aberto: (c) se a Nomeação passa a valer por estabelecimento — hoje a unicidade é por par Tomador×Seguradora; (d) como uma Filial é removida ou desvinculada da matriz (o cadastro nasce sem remoção, como o vínculo de papel da RN-017).


## OPEN-22 — Nome divergente da mesma Cobertura Adicional entre ramos de uma Seguradora
Dono: PO (gerente de projeto)
Bloqueia: o envio da Cobertura Adicional à Seguradora (RN-105) quando a mesma Cobertura Adicional canônica está vinculada a Coberturas Adicionais Importadas com nomes diferentes na mesma Seguradora — tipicamente uma por ramo (Público e Privado)
Status: aberta
Contexto: levantado em 2026-08-04 (AB#0007). Cada Modalidade canônica mapeia para duas Modalidades Importadas por Seguradora, uma por ramo, e cada uma tem as suas próprias Coberturas Adicionais Importadas. Quando os nomes coincidem — o caso observado no catálogo de QA, ex.: "Multas" nos dois ramos da AXA — o envio pelo nome é inequívoco e a regra funciona. Quando divergem (ex.: "Multa" num ramo e "Multas" no outro), não há como escolher sem uma regra de ramo, que a plataforma hoje não tem: o ramo é resolvido pela Seguradora a partir da Modalidade enviada. Enviar os dois nomes não é alternativa — a Seguradora recusa a solicitação inteira se um deles não for suportado no ramo que ela resolver, derrubando a Cotação (ADR-103). Até a decisão, RN-105/RN-106 tratam o caso como não contemplado, que é o lado seguro. Decisões possíveis: definir como a plataforma determina o ramo (por exemplo derivado da natureza do Segurado) ou exigir da curadoria que o vínculo da canônica seja feito por ramo.
