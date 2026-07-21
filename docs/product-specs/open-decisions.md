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

## OPEN-08 — Mapeamento automático de modalidade "por semelhança"
Dono: PO (gerente de projeto)
Bloqueia: a confirmação automática de Mapeamento de Modalidade por aproximação de nome/descrição (forma `Similarity`)
Status: aberta
Contexto: levantado em 2026-07-21 (jornada Catálogo de Modalidades, AB#0002). A importação confirma mapeamento automaticamente apenas "por identificador do motor" (RN-032); tudo o mais cai na Fila de Revisão como pendente (RN-034). A confirmação automática "por semelhança" — aproximar a Modalidade Importada das Modalidades do mesmo Ramo por nome/descrição e confirmar acima de um grau de confiança — fica fora desta fase. Falta a PO definir o método de aproximação e o grau de confiança mínimo (idealmente calibrados com dados reais após a primeira carga). Sem essa definição, "por semelhança" não é implementável.

## OPEN-09 — Credencial e divergência de catálogo quando a Seguradora tem várias Corretoras habilitadas
Dono: PO (gerente de projeto)
Bloqueia: a regra de qual credencial (PlugKey) usar na importação de uma Seguradora habilitada por mais de uma Corretora, e o tratamento caso o catálogo retornado divirja entre Corretoras
Status: aberta
Contexto: levantado em 2026-07-21 (jornada Catálogo de Modalidades, AB#0002). A Modalidade Importada é da Seguradora, não da Corretora (o `BrokerCnpj`/PlugKey é só credencial de busca), então a importação deduplica por Seguradora e faz uma chamada por Seguradora (RN-031). Falta a PO decidir qual credencial usar quando várias Corretoras habilitam a mesma Seguradora e o que fazer se o retorno divergir entre elas (hoje assume-se catálogo único por Seguradora).

## OPEN-10 — Cadência do agendamento da importação de modalidades
Dono: PO (gerente de projeto)
Bloqueia: nada crítico (há default proposto); ajusta a frequência do job
Status: aberta
Contexto: levantado em 2026-07-21 (jornada Catálogo de Modalidades, AB#0002). A importação roda periodicamente por agendamento (RN-031). Proposta de default: diária, em horário de baixo pico, com a cadência configurável (não fixa no código). Falta a PO confirmar se há requisito de frequência específico (ex.: mais de uma vez ao dia, ou alinhado a janela da Seguradora).
