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
Status: parcialmente resolvida em 2026-07-23
Contexto: a criação de Usuário (RN-001) nasce sem vínculo com Corretora e sem perfis, por decisão do negócio nesta fase. Falta decidir quando e como o Usuário passa a pertencer a uma Corretora e quais perfis restringem a criação — hoje qualquer usuário autenticado cria.
Parcialmente resolvida em 2026-07-23 (RN-032..RN-046, jornada Perfis e Permissões): definidos o vínculo Usuário↔Corretora (N por Usuário, cada um portando o Perfil naquela Corretora), a Corretora ativa como gate das Permissões efetivas, o modelo Perfil×Escopo×Permissão, a hierarquia de criação (Admin→Corretor Administrador; Corretor Administrador→Tomador Administrador/Corretor; Tomador Administrador→Tomador; comuns por Permissão) e as verificações de nomeação para o Tomador Administrador. Depende da ratificação da PO dos termos novos e da nova cardinalidade de Perfil no glossário. Segue aberto: o isolamento multi-tenant técnico (query filters ADR-035, claims ADR-014) continua sob esta pendência até implementado.

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
Status: parcialmente resolvida em 2026-07-23
Contexto: decidido em 2026-07-16 que o Usuário Pendente não se autentica (RN-005); a primeira senha será definida pelo próprio Usuário através de link de convite enviado por e-mail — uso único, com prazo de validade (proposta: 7 dias) e reenviável enquanto o Usuário for Pendente. O envio de e-mail ainda não existe e será implementado na funcionalidade de convite, quando RN-001 e RN-002 serão revisadas. Decidido em 2026-07-16: o login recusa o Usuário Pendente sempre (RN-005 literal) — usuário novo permanece sem acesso até a funcionalidade de convite existir; a ativação (RN-002) fica inoperante nesse intervalo por decisão consciente do negócio.
Endereçada em 2026-07-23 (RN-035, jornada Perfis e Permissões): o mecanismo de Convite (link de uso único, validade, reenvio) e o primeiro acesso com senha própria foram especificados, revisando RN-001 (fim da senha inicial padrão) e RN-002 (ativação pelo link). RN-001 e RN-002 foram reescritas como PROPOSTA em 2026-07-23 (bloco de revisão no topo de cada uma em usuarios.md), refletindo o convite. Correção 2026-07-23: o transporte de e-mail JÁ EXISTE (`IMailService`/`MailKitMailService`, ADR-048) — a antiga afirmação de "serviço de e-mail inexistente" está superada. Restante pendente da PO: confirmar o prazo de validade do link (proposta 7 dias) e ratificar a revisão de RN-001/RN-002. O mecanismo de convite (geração/validação do link de uso único e o caso de uso de envio) é implementação, não bloqueio.

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

## OPEN-09 — Nomes técnicos dos Perfis fixos (colisão com Papel da Pessoa)
Dono: PO (gerente de projeto) + time (ADR-058, dono do vocabulário)
Bloqueia: código de domínio dos Perfis fixos Corretor e Tomador da jornada Perfis e Permissões (RN-032..RN-046)
Status: aberta
Contexto: os Perfis fixos "Corretor" e "Tomador" (papéis de acesso do Usuário) colidem em nome com o Papel da Pessoa Corretor (`Broker`) e Tomador (`PolicyHolder`), que já existem no glossário e são conceitos distintos (o papel da Pessoa não é o Perfil do Usuário). Precisa a PO/time decidir os nomes técnicos 1:1 desses dois Perfis fixos (ex.: `BrokerProfile`/`PolicyHolderProfile` ou outro) antes de qualquer código, sob a regra do ADR-058. Corretor Administrador e Tomador Administrador já têm proposta de nome técnico no glossário (`BrokerageAdministrator`/`PolicyHolderAdministrator`), também sujeita a ratificação.

## OPEN-10 — Remoção de Permissão essencial à própria administração
Dono: PO (gerente de projeto)
Bloqueia: o comportamento da edição de Perfil fixo (RN-043) quando a Permissão removida é a que sustenta a administração (ex.: gerenciar Usuários no Corretor Administrador)
Status: aberta
Contexto: a RN-043 permite ao Administrador do Sistema editar as Permissões dos Perfis fixos com efeito global. Não foi decidido o que a plataforma faz se ele remover a própria Permissão que sustenta a administração de um Escopo — se bloqueia (como a RN-046 faz ao impedir Escopo sem administrador), se avisa, ou se apenas registra. Sem precedente ratificado, a RN-043 declara o efeito como não definido nesta fase.

## OPEN-11 — Mecânica do Escopo ativo (Corretora/Tomador ativo) e escopo padrão no primeiro acesso
Dono: arquitetura (ADR) + PO (comportamento no primeiro acesso)
Bloqueia: a resolução do Escopo ativo em tempo de request na RN-034 (permissões efetivas por Corretora/Tomador ativo) e, por consequência, os query filters multi-tenant por Corretora ativa (ADR-035). Não bloqueia as tabelas/entidades de vínculo (Usuário↔Corretora/Tomador), que são o N:N do glossário e podem nascer antes.
Status: aberta (direção proposta em 2026-07-23)
Direção proposta (2026-07-23): a mecânica candidata é Escopo ativo carregado como claim do acesso (ADR-060, status proposto — aguardando ratificação do dono de arquitetura), coerente com o ADR-014; troca reemite o acesso validando o vínculo; sem tabela de sessão nova. Segue ABERTO: (a) ratificação da mecânica pela arquitetura (ADR-060); (b) dono PO/UX — qual o Escopo ativo padrão no primeiro acesso quando o Usuário tem mais de um vínculo (a única vira ativa automaticamente? seleção obrigatória antes de operar?). A fatia 1b (carregamento/troca da claim) fica pendente de (a) e (b); a fatia 1a (vínculos) não depende delas.

## OPEN-12 — Autoridade de inativação por escopo e Usuário multi-Corretora (RN-046)
Dono: PO (gerente de projeto)
Bloqueia: a inativação/reativação de Usuário pelos atores de escopo da RN-046 (Corretor Administrador, Tomador Administrador, usuário comum com permissão) — a fatia entregue faz apenas a do Administrador do Sistema.
Status: aberta
Contexto: a inativação torna o Usuário Inativo globalmente ("Usuário Inativo não acessa a plataforma"), mas um Usuário pode estar vinculado a várias Corretoras/Tomadores (RN-034). A RN-046 diz que o Corretor Administrador inativa "usuários das suas corretoras" — não está resolvido se um CA pode inativar GLOBALMENTE um Usuário que também pertence a outra Corretora (de outro CA), ou se a ação deveria ser apenas a remoção do vínculo naquele escopo. Também depende do enforcement por permissão (RN-033, adiado) para o caso do usuário comum. Enquanto aberto, só o Administrador do Sistema inativa/reativa (global, sem ambiguidade). Decidir a semântica (global vs por escopo) antes de estender aos demais atores.
Inclui também o **guard do lado do alvo** da RN-046 ("inativação que deixaria uma Corretora ou Tomador sem nenhum administrador é recusada"): a fatia entregue implementa apenas o guard de Escopo System (não deixar a plataforma sem Administrador do Sistema); a proteção equivalente para Corretora (último Corretor Administrador) e Tomador (último Tomador Administrador) fica com esta decisão — depende de contagem de administradores ativos por Escopo, que só faz sentido junto com a semântica global-vs-escopo. Limitação atual consciente: o Administrador do Sistema pode inativar o último Corretor/Tomador Administrador de um Escopo.
Contexto: a RN-034 exige que, com o Usuário vinculado a mais de uma Corretora/Tomador, exista uma Corretora ativa e um Tomador ativo que determinam as Permissões efetivas do momento. Nenhum ADR define COMO isso é resolvido por request. Candidatos: (a) claim no JWT (stateless, exige reemissão do token ao trocar); (b) sessão/estado no servidor (tabela de sessão com escopo ativo por token); (c) header por request (cliente envia a cada chamada); (d) híbrido (header com fallback pra sessão). Cada um afeta segurança (fonte de verdade da permissão), reemissão de token, consistência multi-aba e o gancho do multi-tenant (ADR-035). Também em aberto: qual o Escopo ativo padrão no primeiro acesso (única Corretora vinculada vira ativa automaticamente? seleção obrigatória?) — parte com sabor de PO. Decisão vira ADR (mecânica) e, se necessário, ajuste de RN-034 (comportamento padrão). Enquanto aberta, os vínculos podem ser implementados, mas a seleção/uso do Escopo ativo não.
