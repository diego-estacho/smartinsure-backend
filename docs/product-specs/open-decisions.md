# Decisões abertas

O que está listado aqui **não é implementável** até ser decidido. Agente que encontrar uma dependência aberta: pare e aponte a decisão, não invente. Decisão tomada vira ADR (ou atualização do glossário/RN) e a entrada sai daqui. A lista cresce conforme o trabalho real esbarra em bloqueio — não é um backlog planejado.

## OPEN-01 — Glossário canônico e máquina de estados
Dono: PO (gerente de projeto)
Bloqueia: qualquer código de domínio (nomes de entidades, rotas, telas, status)
Status: aberta
Contexto: a proposta está em [glossario.md](glossario.md). Falta a PO ratificar os termos e enumerar os status do produto com as transições permitidas.

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

## OPEN-04 — Uso dos dados retornados pelo Bureau
Dono: PO (gerente de projeto)
Bloqueia: qualquer efeito automático dos dados do Bureau (preencher cadastro, bloquear ou alertar por situação cadastral) e os gatilhos de negócio que disparam a consulta
Status: aberta
Contexto: a consulta ao Bureau existe como serviço reutilizável (RN-003, RN-004), mas o negócio ainda não definiu o que a plataforma faz com o retorno (situação cadastral, endereço, atividade econômica) nem em quais momentos da jornada a consulta dispara. Cada consulta tem custo por chamada e hoje não há reuso de respostas — se o volume crescer, a decisão de reuso/validade também é da PO.
