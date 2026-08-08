# Jornada: Usuários

## RN-001 — Criação de Usuário

> Revisão proposta em 2026-07-23 (grill-rn, jornada Perfis e Permissões / [OPEN-06](../open-decisions.md)): a senha inicial padrão dá lugar ao primeiro acesso por Convite (RN-065). Aguardando ratificação da PO — o texto abaixo já reflete a proposta.
>
> Revisão 2026-08-07 (redesenho da tela de Usuários, decisão do dono do produto): os fluxos de convite que criam Usuário para operar em Corretora ou Tomador (RN-068/RN-069/RN-070) passam a informar também o **CPF** do convidado — ver RN-082. O convite de Corretor Administrador (RN-066) não coleta CPF nesta fase.

**Descrição.** A criação de Usuário é feita pelos fluxos de criação/convite da jornada Perfis e Permissões (RN-066, RN-068 a RN-071), informando nome e e-mail (e o CPF, nos fluxos do §8 — RN-082). O Usuário nasce com identidade registrada no provedor de identidade e na situação Pendente, sem senha própria — o primeiro acesso ocorre por Convite (RN-065).

**Pré-condições.** Solicitante autenticado e autorizado a criar Usuário no Escopo em questão (RN-066, RN-068 a RN-071); e-mail informado não cadastrado na plataforma nem no provedor de identidade.

**Critério de aceitação.** Ao criar um Usuário com nome e e-mail válidos, ele passa a existir na plataforma guardando a referência ao identificador da sua identidade no provedor de identidade, na situação Pendente, sem senha própria (a identidade nasce sem credencial de acesso utilizável pelo Usuário — a senha é definida por ele no primeiro acesso), e recebe um Convite para o primeiro acesso (RN-065). Em nenhum momento existe Usuário na plataforma sem identidade correspondente no provedor de identidade.

**Nome de usuário no provedor de identidade.** A identidade nasce com nome de usuário derivado do e-mail, no formato aceito pelo provedor (apenas letras, números e underline): prefixo do ambiente (ex.: `dev_insp`) seguido de underline e do e-mail com todo caractere não alfanumérico substituído por underline; o resultado é limitado a 39 caracteres, não termina em underline (underline final vira `0`) e fica em minúsculas. O e-mail permanece o identificador de negócio; o nome de usuário é detalhe de integração e não é exibido na plataforma. Mesma derivação usada no InsurePoint legado.

**Casos limite.** Nome ou e-mail ausentes ou inválidos: criação recusada. E-mail já cadastrado na plataforma: criação recusada. E-mail já existente no provedor de identidade: criação recusada — a identidade preexistente não é adotada nem alterada. Falha ao registrar o Usuário na plataforma após a identidade ter sido criada no provedor: a identidade recém-criada é desfeita e a criação é recusada, sem deixar identidade órfã. E-mails distintos podem colidir no nome de usuário após o corte de 39 caracteres: o provedor exige nome de usuário único e recusa a criação — a criação é recusada sem identidade órfã, como em qualquer recusa do provedor.

## RN-002 — Ativação do Usuário no primeiro acesso

> Revisão proposta em 2026-07-23 (grill-rn, jornada Perfis e Permissões / [OPEN-06](../open-decisions.md)): a ativação passa a ocorrer pelo link de Convite (RN-065), sem senha inicial padrão nem autenticação prévia. Aguardando ratificação da PO — o texto abaixo já reflete a proposta.

**Descrição.** O Usuário na situação Pendente torna-se Ativo ao concluir o primeiro acesso pelo link de Convite (RN-065), definindo uma senha própria.

**Pré-condições.** Usuário na situação Pendente, portador de um Convite com link válido (RN-065).

**Critério de aceitação.** Ao abrir o link de Convite válido e definir a própria senha, a situação do Usuário passa de Pendente para Ativo. Enquanto o primeiro acesso não for concluído, o Usuário permanece Pendente e não acessa a plataforma (RN-005).

**Casos limite.** Link de Convite usado, expirado ou substituído por reenvio: primeiro acesso recusado, orientando solicitar novo Convite (RN-065); o Usuário permanece Pendente. Tentativa de acesso sem concluir o primeiro acesso: recusada, o Usuário permanece Pendente.

## RN-005 — Autenticação de Usuário com e-mail e senha

**Descrição.** O Usuário na situação Ativo acessa a plataforma informando e-mail e senha. As credenciais são validadas exclusivamente no provedor de identidade — a plataforma não guarda nem valida senhas — e, quando válidas, o Usuário recebe acesso autenticado com validade de 8 horas.

**Pré-condições.** Usuário na situação Ativo, existente na plataforma e com identidade correspondente no provedor de identidade.

**Critério de aceitação.** Ao informar e-mail e senha reconhecidos pelo provedor de identidade, o Usuário Ativo obtém acesso autenticado à plataforma, válido por 8 horas; vencido esse prazo, um novo acesso exige nova autenticação. A validação da senha ocorre somente no provedor de identidade.

**Casos limite.** E-mail ou senha incorretos: acesso recusado com uma única mensagem que não revela se o e-mail está cadastrado. Usuário na situação Pendente: acesso recusado — o primeiro acesso acontece pelo fluxo de convite ([OPEN-06](../open-decisions.md)). Credenciais aceitas pelo provedor de identidade, mas sem Usuário correspondente na plataforma: acesso recusado com a mesma mensagem de credenciais incorretas. Provedor de identidade indisponível: acesso recusado com mensagem de indisponibilidade, distinta da de credenciais incorretas; acessos autenticados já concedidos permanecem válidos até o fim das suas 8 horas (salvo encerramento pelo próprio Usuário — RN-006). Bloqueio por tentativas repetidas de acesso: não há nesta fase ([OPEN-05](../open-decisions.md)).

## RN-006 — Encerramento de sessão

**Descrição.** O Usuário autenticado pode encerrar sua sessão a qualquer momento. A partir do encerramento, aquele acesso autenticado deixa de ser aceito pela plataforma imediatamente, mesmo antes do fim das 8 horas de validade (RN-005).

**Pré-condições.** Usuário portador de um acesso autenticado válido (RN-005).

**Critério de aceitação.** Após o encerramento, qualquer chamada à plataforma com o mesmo acesso é recusada como não autenticada. Encerrar uma sessão já encerrada não tem efeito adicional (idempotente).

**Casos limite.** Acesso já expirado no momento do encerramento: sem efeito — a recusa já decorre da expiração. O encerramento vale só para aquele acesso: outros acessos do mesmo Usuário permanecem válidos até expirar ou serem encerrados. O provedor de identidade não participa do encerramento — a sessão é da plataforma.

## RN-012 — Perfil Administrador do Sistema

**Descrição.** O Usuário pode ter o Perfil Administrador do Sistema, que autoriza as operações internas da plataforma (como manter o catálogo de Seguradoras). Usuário sem Perfil é usuário comum. Somente um Administrador do Sistema concede ou revoga o Perfil de outro Usuário; o primeiro Administrador do Sistema nasce por operação interna da equipe SmartInsure.

**Pré-condições.** Concedente autenticado com o perfil Administrador do Sistema; Usuário destinatário existente na plataforma.

**Critério de aceitação.** Ao conceder o Perfil a um Usuário, ele passa a poder executar as operações exclusivas do Perfil; ao revogar, deixa de poder executá-las imediatamente. Concessão ou revogação solicitada por Usuário sem o Perfil é recusada por falta de permissão.

**Casos limite.** Conceder o Perfil a quem já o tem, ou revogar de quem não o tem: solicitação recusada com indicação clara de que o Usuário já está na condição pedida. Revogação que deixaria a plataforma sem nenhum Administrador do Sistema: recusada. Usuário destinatário inexistente: recusada com indicação clara.

## RN-082 — CPF do Usuário

> Proposta em 2026-08-07 (redesenho da tela de Usuários, decisão do dono do produto). Autorizado a ajustar; aguardando ratificação da PO.

**Descrição.** O Usuário criado pelos fluxos de convite destinados a operar em Corretora ou Tomador (RN-068/RN-069/RN-070) informa também o seu CPF, que identifica a pessoa por trás do acesso. O CPF é único na plataforma e imutável — se está errado, é outro cadastro. O convite de Corretor Administrador (RN-066) não coleta CPF nesta fase, e Usuários criados antes desta regra permanecem sem CPF.

**Pré-condições.** Fluxo de convite de Corretora/Tomador (RN-068/RN-069/RN-070) com CPF informado.

**Critério de aceitação.** Nesses fluxos o CPF é obrigatório e validado na forma (11 dígitos com dígitos verificadores corretos), guardado somente em dígitos. Dois Usuários não têm o mesmo CPF. O CPF não é alterável depois de criado (a correção é um novo cadastro). Usuário sem CPF (Corretor Administrador ou anterior à regra) é válido e não colide no critério de unicidade. A busca da listagem de Usuários passa a encontrar por CPF.

**Casos limite.** CPF ausente ou inválido nesses fluxos: convite recusado. CPF já usado por outro Usuário: recusado. CPF informado com máscara: aceito e normalizado para dígitos. Convite de Corretor Administrador (RN-066): segue sem CPF por decisão de produto do redesenho.

## RN-202 — Edição de Usuário

> Proposta em 2026-08-08 (redesenho da tela de Usuários, decisão do dono do produto; faixa 200 escolhida para não colidir com a numeração da jornada Perfis). Autorizado a ajustar; aguardando ratificação da PO.

**Descrição.** O nome de cadastro do Usuário pode ser corrigido a qualquer momento, preservando o histórico. O e-mail — credencial de acesso (RN-005) — só pode ser corrigido enquanto o Usuário está Pendente (antes do primeiro acesso, quando ainda não há credencial): a correção atualiza a identidade no provedor e reenvia o Convite para o novo endereço (RN-065). O CPF (RN-082) é imutável. A troca de Perfil segue as regras próprias (RN-012 no Escopo Sistema; RN-075 no vínculo de Corretora/Tomador).

**Pré-condições.** Solicitante autorizado a gerenciar o Usuário; para corrigir o e-mail, Usuário na situação Pendente.

**Critério de aceitação.** Ao salvar, o nome é atualizado. Informando um novo e-mail para um Usuário Pendente: verifica-se a unicidade (plataforma e provedor de identidade), a identidade tem o e-mail atualizado, o Usuário permanece Pendente e recebe um novo Convite no novo endereço (o link anterior deixa de valer). Corrigir o e-mail de um Usuário Ativo ou Inativo é recusado.

**Casos limite.** E-mail igual ao atual: só o nome muda. Novo e-mail já usado por outro Usuário ou identidade: recusado. Alterar o e-mail de Usuário não-Pendente: recusado, orientando inativar e convidar o novo endereço. CPF não é alterável (RN-082).

## RN-203 — Redefinição de senha do Usuário Ativo

> Proposta em 2026-08-08 (redesenho da tela de Usuários, decisão do dono do produto; faixa 200). Autorizado a ajustar; aguardando ratificação da PO.

**Descrição.** Um administrador pode disparar a redefinição de senha de um Usuário **Ativo** — o caso "o usuário esqueceu a senha" ou "precisa trocar". O sistema gera um link de redefinição de **uso único e com validade** (mesma mecânica do Convite, RN-065) e o envia por e-mail ao endereço do Usuário. Ao abrir o link e definir a nova senha, ela é atualizada no provedor de identidade (RN-005); o Usuário **permanece Ativo** e o histórico é preservado. A plataforma não guarda nem transmite a senha (nunca envia senha por e-mail — só o link para o próprio Usuário definir a sua).

**Pré-condições.** Solicitante autorizado a gerenciar o Usuário; Usuário na situação **Ativo**.

**Critério de aceitação.** Ao acionar a redefinição de um Usuário Ativo, gera-se um link de redefinição válido e envia-se um e-mail ao endereço do Usuário; um pedido de redefinição anterior ainda válido deixa de valer (um link ativo por Usuário, como no Convite). O Usuário continua Ativo. Ao concluir pelo link, a senha é atualizada no provedor de identidade; os acessos autenticados já concedidos seguem válidos até expirar ou serem encerrados (RN-005/RN-006). O link de redefinição segue as regras de senha do primeiro acesso (mínimo de 8 caracteres).

**Casos limite.** Usuário **Pendente**: recusado — ele ainda não definiu senha; o caminho é reenviar o Convite de primeiro acesso (RN-065). Usuário **Inativo**: recusado — não acessa a plataforma; reative antes (RN-076). Falha no envio do e-mail: o link foi gerado e a operação pode ser repetida (o pedido anterior deixa de valer). Link de redefinição expirado ou já usado: recusado ao concluir, orientando solicitar novo.
