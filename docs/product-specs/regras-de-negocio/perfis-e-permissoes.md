# Jornada: Perfis e Permissões

> Proposta em 2026-07-23 (grill-rn). Endereça a OPEN-03 (vínculo Usuário↔Corretora e perfis que restringem a criação) e a OPEN-06 (primeiro acesso por convite). Depende de ratificação da PO: os termos novos e a mudança de cardinalidade de **Perfil** no [glossário](../glossario.md), o status **Inativo** de Usuário, e a resolução dos nomes técnicos em conflito (ver [OPEN-09](../open-decisions.md)). IDs provisórios até a aprovação.

## RN-032 — Perfil como conjunto de Permissões com Escopo

**Descrição.** Um Perfil é um conjunto nomeado de Permissões que autoriza operações da plataforma, atrelado a um Escopo: Sistema, uma Corretora ou um Tomador. Existem Perfis fixos — Administrador do Sistema (RN-012), Corretor Administrador, Tomador Administrador, Corretor e Tomador — que a plataforma sempre oferece, e Perfis customizados criados pelos administradores de Corretora ou de Tomador. Dentro de uma Corretora, o Usuário tem no máximo um Perfil.

**Pré-condições.** Modelo de Perfil e Escopo ratificado no glossário; Permissões disponíveis no catálogo (RN-033).

**Critério de aceitação.** Todo Perfil carrega um Escopo e uma lista de Permissões; um Perfil de Escopo Corretora só vale dentro da Corretora à qual pertence e um de Escopo Tomador só dentro daquele Tomador. As operações que um Usuário pode executar numa Corretora são exatamente as Permissões do Perfil que ele tem naquela Corretora. Um Usuário nunca tem mais de um Perfil na mesma Corretora.

**Casos limite.** Perfil sem nenhuma Permissão: válido, mas não autoriza operação alguma. Perfil fixo Administrador do Sistema tem Escopo Sistema e não se vincula a Corretora nem a Tomador. Tentativa de atribuir a um Usuário um segundo Perfil na mesma Corretora: recusada — a atribuição substitui o Perfil anterior (RN-045), nunca soma.

## RN-033 — Catálogo fixo de Permissões

**Descrição.** As Permissões que compõem os Perfis vêm de um catálogo fixo da plataforma: cada funcionalidade declara as Permissões que ela exige. Ninguém cria Permissão pela tela — o catálogo é definido pela própria plataforma; ao editar um Perfil, o administrador apenas marca ou desmarca Permissões desse catálogo.

**Pré-condições.** Funcionalidade que exige controle de acesso declarou sua Permissão no catálogo.

**Critério de aceitação.** A lista de Permissões oferecida na edição de qualquer Perfil é exatamente o catálogo declarado pelas funcionalidades, sem inclusão manual. Uma funcionalidade só executa para o Usuário se o Perfil dele na Corretora ativa contiver a Permissão que ela exige.

**Casos limite.** Permissão presente no catálogo mas não marcada em nenhum Perfil: a funcionalidade correspondente fica indisponível para todos até ser marcada. Funcionalidade sem Permissão declarada: fora do controle de acesso por Perfil (não é gate desta jornada).

## RN-034 — Vínculos do Usuário com Corretoras e Tomadores e o Escopo ativo

**Descrição.** Um Usuário pode estar vinculado a várias Corretoras e a vários Tomadores; em cada vínculo tem um Perfil próprio (RN-032). Quando vinculado a mais de uma Corretora, o Usuário escolhe a Corretora ativa; quando vinculado a mais de um Tomador, escolhe o Tomador ativo. As operações e as Permissões efetivas em cada momento são as do Perfil do Escopo ativo correspondente.

**Pré-condições.** Usuário existente com ao menos um vínculo de Corretora ou de Tomador.

**Critério de aceitação.** As Permissões efetivas do Usuário sobre dados de uma Corretora são as do Perfil dele na Corretora ativa; sobre dados de um Tomador, as do Perfil dele no Tomador ativo. Ao trocar a Corretora ativa ou o Tomador ativo, as Permissões passam a ser as do Perfil no novo Escopo. Operação sobre dados de uma Corretora ou de um Tomador só é permitida quando aquele é o Escopo ativo e o Perfil ali a autoriza.

**Casos limite.** Usuário vinculado a uma única Corretora (ou a um único Tomador): ela/ele é sempre o ativo, sem escolha. Usuário sem vínculo de Corretora nem de Tomador (ex.: apenas Administrador do Sistema): opera apenas as operações de Escopo Sistema. Troca de Escopo ativo não altera vínculos nem Perfis — só muda o contexto em uso.

## RN-035 — Primeiro acesso do Usuário por convite

**Descrição.** Todo Usuário criado na plataforma passa a existir na situação Pendente e recebe um Convite por e-mail com um link de uso único e prazo de validade. O primeiro acesso ocorre por esse link, onde o Usuário define a própria senha; concluída a definição, o Usuário passa a Ativo. Substitui a senha inicial padrão da criação de Usuário (revisa RN-001 e RN-002).

**Pré-condições.** Usuário na situação Pendente, recém-criado por qualquer fluxo de criação desta jornada; serviço de envio de e-mail disponível.

**Critério de aceitação.** Ao criar um Usuário, um Convite é enviado ao e-mail informado com link de uso único e validade definida. Ao abrir o link válido e definir a própria senha, o Usuário passa de Pendente para Ativo e obtém acesso. Enquanto não concluir, permanece Pendente e não acessa a plataforma (RN-005). O Convite é reenviável enquanto o Usuário for Pendente, invalidando o link anterior.

**Casos limite.** Link já usado, expirado ou substituído por reenvio: acesso recusado, orientando solicitar novo Convite. Convite de Usuário que já está Ativo: recusado. Falha no envio do e-mail: a criação do Usuário é reportada como incompleta, permitindo reenvio — o Usuário permanece Pendente. Prazo de validade do link e reenvio seguem a decisão registrada na [OPEN-06](../open-decisions.md).

## RN-036 — Administrador do Sistema convida Corretor Administrador

**Descrição.** Somente o Administrador do Sistema convida novos Usuários para o Perfil Corretor Administrador. No ato do Convite, ele informa a quais Corretoras o convidado passa a estar vinculado. O Administrador do Sistema não convida nem cria Usuários de outros Perfis por este fluxo.

**Pré-condições.** Solicitante autenticado com o Perfil Administrador do Sistema; ao menos uma Corretora informada no Convite.

**Critério de aceitação.** Ao convidar, o Usuário nasce Pendente (RN-035) com o Perfil Corretor Administrador em cada Corretora informada. O convite feito por Usuário sem o Perfil Administrador do Sistema é recusado por falta de permissão. Convite para qualquer Perfil que não seja Corretor Administrador, por este fluxo, é recusado.

**Casos limite.** Nenhuma Corretora informada: convite recusado. Corretora informada inexistente ou Inativa: convite recusado com indicação clara. E-mail já cadastrado na plataforma: recusado (RN-001).

## RN-037 — Atribuição de Corretoras a um Usuário existente

**Descrição.** As Corretoras vinculadas a um Usuário podem ser ampliadas após a criação. O Administrador do Sistema atribui qualquer Corretora a qualquer Usuário. O Corretor Administrador atribui apenas as Corretoras às quais ele próprio pertence, e apenas a Usuários dessas Corretoras.

**Pré-condições.** Solicitante autenticado com Perfil que autorize a atribuição (Administrador do Sistema, ou Corretor Administrador para as próprias Corretoras); Usuário destinatário existente.

**Critério de aceitação.** Ao atribuir uma Corretora a um Usuário, ele passa a ter vínculo e Perfil naquela Corretora e pode selecioná-la como ativa (RN-034). Corretor Administrador que tente atribuir Corretora à qual não pertence, ou a Usuário fora de suas Corretoras, é recusado por falta de permissão.

**Casos limite.** Corretora já vinculada ao Usuário: atribuição recusada com indicação de que já existe o vínculo. Ao atribuir, o Perfil naquela Corretora é definido segundo as regras de criação (RN-038 a RN-041) conforme o Perfil pretendido.

## RN-038 — Corretor Administrador cria Tomador Administrador

**Descrição.** O Corretor Administrador cria Usuários com o Perfil Tomador Administrador para um Tomador. A atribuição exige que o Tomador escolhido tenha Nomeação de Tomador Vigente em que a Corretora ativa é a nomeada; a verificação usa sempre a Corretora ativa do Corretor Administrador.

**Pré-condições.** Solicitante autenticado com o Perfil Corretor Administrador; Corretora ativa selecionada; Tomador escolhido com Nomeação de Tomador Vigente tendo a Corretora ativa como nomeada.

**Critério de aceitação.** Ao criar um Tomador Administrador, verifica-se que o Tomador escolhido tem ao menos uma Nomeação de Tomador Vigente em que a Corretora ativa é a nomeada, em qualquer Seguradora; havendo, o Usuário nasce Pendente (RN-035) com o Perfil Tomador Administrador vinculado àquele Tomador. Sem Nomeação Vigente correspondente, a criação é recusada.

**Casos limite.** Tomador sem nenhuma Nomeação Vigente na Corretora ativa: recusado. Nomeação Vigente do Tomador existe, mas com outra Corretora como nomeada: recusado. Corretor Administrador com mais de uma Corretora: a verificação considera exclusivamente a Corretora ativa no momento, não as demais.

## RN-039 — Corretor Administrador cria Corretores e Perfis de contexto Corretora

**Descrição.** O Corretor Administrador cria Usuários com o Perfil Corretor na Corretora ativa e cria Perfis customizados de Escopo Corretora. O Perfil customizado nasce vinculado à Corretora ativa e passa a ser oferecido na criação de Usuários daquela Corretora.

**Pré-condições.** Solicitante autenticado com o Perfil Corretor Administrador; Corretora ativa selecionada.

**Critério de aceitação.** Ao criar um Usuário Corretor, ele nasce Pendente (RN-035) vinculado à Corretora ativa com o Perfil escolhido dentre os Perfis de Escopo daquela Corretora. Ao criar um Perfil customizado, ele fica vinculado à Corretora ativa e disponível apenas na criação de Usuários dessa Corretora. Perfis customizados de Escopo Tomador não são criados por este fluxo.

**Casos limite.** Corretor Administrador com várias Corretoras: o Perfil e o Usuário criados pertencem somente à Corretora ativa. Nome de Perfil repetido dentro da mesma Corretora: criação recusada com indicação clara.

## RN-040 — Tomador Administrador cria Tomadores e Perfis de contexto Tomador

**Descrição.** O Tomador Administrador cria Usuários com o Perfil Tomador para o Tomador ativo e cria Perfis customizados de Escopo Tomador. O Perfil customizado nasce vinculado ao Tomador ativo e passa a ser oferecido apenas na criação de Usuários daquele Tomador.

**Pré-condições.** Solicitante autenticado com o Perfil Tomador Administrador; Tomador ativo selecionado dentre os Tomadores que ele administra.

**Critério de aceitação.** Ao criar um Usuário Tomador, ele nasce Pendente (RN-035) vinculado ao Tomador ativo do solicitante, com o Perfil escolhido dentre os Perfis de Escopo daquele Tomador. Ao criar um Perfil customizado, ele fica vinculado ao Tomador ativo e disponível apenas na criação de Usuários do mesmo Tomador — outro administrador do mesmo Tomador vê esses Perfis; administradores de outros Tomadores não.

**Casos limite.** Nome de Perfil repetido dentro do mesmo Tomador: criação recusada. Tomador Administrador não cria Perfis de Escopo Corretora nem de Escopo Sistema.

## RN-041 — Criação de Usuário por Usuário comum depende de Permissão e do próprio Escopo

**Descrição.** Usuários com Perfil Corretor ou Tomador também criam novos Usuários, desde que o Perfil no Escopo ativo contenha a Permissão de criar Usuário. Nesse caso não há seleção de Corretora nem de Tomador: o novo Usuário herda o Escopo ativo do criador — a Corretora ativa, para o Corretor, e o Tomador ativo, para o Tomador.

**Pré-condições.** Solicitante autenticado cujo Perfil no Escopo ativo (Corretora ativa ou Tomador ativo) contém a Permissão de criar Usuário.

**Critério de aceitação.** O Usuário criado nasce Pendente (RN-035) no mesmo Escopo ativo do criador, sem que Corretora ou Tomador sejam oferecidos para escolha, e recebe um Perfil dentre os oferecidos para aquele Escopo (RN-042). Solicitante sem a Permissão de criar Usuário tem a criação recusada por falta de permissão.

**Casos limite.** Corretor cuja Corretora ativa não concede a Permissão: recusado, mesmo que outra Corretora do mesmo Usuário concedesse (o mesmo vale para o Tomador ativo). Nenhum Perfil disponível no Escopo do criador: criação recusada com indicação clara.

## RN-042 — Visibilidade dos Perfis na criação de Usuário

**Descrição.** Na criação de um Usuário, os Perfis oferecidos são apenas os do Escopo daquele contexto: no contexto de Corretora, os Perfis fixos de Corretora mais os Perfis customizados vinculados à Corretora ativa; no contexto de Tomador, os Perfis fixos de Tomador mais os Perfis customizados vinculados àquele Tomador.

**Pré-condições.** Fluxo de criação de Usuário em um Escopo determinado (Corretora ativa ou Tomador vinculado).

**Critério de aceitação.** A lista de Perfis apresentada na criação contém exatamente os Perfis do Escopo em questão — nunca Perfis customizados de outra Corretora ou de outro Tomador. Perfis customizados de uma Corretora não aparecem para outra Corretora; os de um Tomador não aparecem para outro Tomador.

**Casos limite.** Corretora sem Perfis customizados: são oferecidos apenas os Perfis fixos aplicáveis ao Escopo. Perfil customizado removido (RN-044) deixa de aparecer imediatamente.

## RN-043 — Edição das Permissões de Perfil fixo pelo Administrador do Sistema

**Descrição.** Os Perfis fixos (Corretor Administrador, Tomador Administrador, Corretor, Tomador) não podem ter estrutura, nome ou Escopo alterados, mas o Administrador do Sistema pode adicionar ou remover Permissões deles. A alteração de um Perfil fixo vale globalmente, para todas as Corretoras e Tomadores.

**Pré-condições.** Solicitante autenticado com o Perfil Administrador do Sistema.

**Critério de aceitação.** Ao adicionar ou remover uma Permissão de um Perfil fixo, a mudança passa a valer imediatamente para todos os Usuários que têm aquele Perfil, em qualquer Corretora ou Tomador. Edição de Perfil fixo por Usuário sem o Perfil Administrador do Sistema é recusada por falta de permissão.

**Casos limite.** Tentativa de renomear, mudar o Escopo ou excluir um Perfil fixo: recusada. Efeito da remoção de uma Permissão essencial à própria administração (ex.: gerenciar Usuários no Perfil Corretor Administrador): comportamento não definido nesta fase ([OPEN-10](../open-decisions.md)).

## RN-044 — Edição e remoção de Perfil customizado

**Descrição.** O Perfil customizado é editado e removido dentro do seu Escopo: os de Corretora, pelo Corretor Administrador da Corretora à qual pertencem; os de Tomador, pelo Tomador Administrador do Tomador ao qual pertencem. A remoção é bloqueada enquanto o Perfil tiver Usuários.

**Pré-condições.** Solicitante autenticado com o Perfil administrador do Escopo do Perfil (Corretor Administrador da Corretora, ou Tomador Administrador do Tomador).

**Critério de aceitação.** A edição altera nome e Permissões do Perfil, valendo dentro do seu Escopo. A remoção só é permitida quando nenhum Usuário tem aquele Perfil; havendo Usuários, é recusada com indicação de que o Perfil está em uso. Edição ou remoção por quem não administra aquele Escopo é recusada por falta de permissão.

**Casos limite.** Perfil customizado com Usuários vinculados: remoção recusada até que os Usuários sejam movidos para outro Perfil (RN-045). Corretor Administrador não edita Perfil de outra Corretora nem de Tomador; Tomador Administrador não edita Perfil de outro Tomador nem de Corretora.

## RN-045 — Troca do Perfil de um Usuário na Corretora ou Tomador

**Descrição.** O Perfil de um Usuário em um Escopo pode ser trocado por outro Perfil do mesmo Escopo. A troca é uma substituição direta: o Usuário passa a ter o novo Perfil e deixa de ter o anterior, sem jamais ficar sem Perfil naquele Escopo.

**Pré-condições.** Solicitante autenticado com Permissão de gerenciar Usuários no Escopo; Usuário destinatário com vínculo naquele Escopo; novo Perfil pertencente ao mesmo Escopo.

**Critério de aceitação.** Ao trocar o Perfil, o Usuário passa imediatamente a operar segundo as Permissões do novo Perfil naquele Escopo e perde as do anterior. Não é possível concluir a troca deixando o Usuário sem Perfil no Escopo.

**Casos limite.** Novo Perfil de Escopo diferente do vínculo: troca recusada. Troca solicitada por quem não administra o Escopo: recusada por falta de permissão. Trocar para o mesmo Perfil já atribuído: sem efeito.

## RN-046 — Inativação e reativação de Usuário

**Descrição.** Um Usuário pode ser inativado e reativado. O Administrador do Sistema age sobre qualquer Usuário; o Corretor Administrador sobre Usuários das suas Corretoras; o Tomador Administrador sobre Usuários do seu Tomador; usuários comuns apenas com a Permissão correspondente, no próprio Escopo. Usuário Inativo não acessa a plataforma.

**Pré-condições.** Solicitante autenticado com autorização no Escopo do Usuário destinatário; Usuário destinatário existente.

**Critério de aceitação.** Ao inativar, o Usuário passa à situação Inativo e qualquer acesso é recusado (as sessões vigentes seguem a política de encerramento — RN-006); ao reativar, volta à situação anterior à inativação e pode acessar novamente. Ação sobre Usuário fora do Escopo do solicitante é recusada por falta de permissão.

**Casos limite.** Inativar Usuário já Inativo, ou reativar Usuário já ativo: recusado com indicação clara. Inativação que deixaria uma Corretora ou Tomador sem nenhum administrador: recusada. Situação Inativo depende da ratificação do status pela PO ([OPEN-01](../open-decisions.md)).
