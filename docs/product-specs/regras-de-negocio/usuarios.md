# Jornada: Usuários

## RN-001 — Criação de Usuário

> Revisão proposta em 2026-07-23 (grill-rn, jornada Perfis e Permissões / [OPEN-06](../open-decisions.md)): a senha inicial padrão dá lugar ao primeiro acesso por Convite (RN-035). Aguardando ratificação da PO — o texto abaixo já reflete a proposta.

**Descrição.** A criação de Usuário é feita pelos fluxos de criação/convite da jornada Perfis e Permissões (RN-036, RN-038 a RN-041), informando nome e e-mail. O Usuário nasce com identidade registrada no provedor de identidade e na situação Pendente, sem senha própria — o primeiro acesso ocorre por Convite (RN-035).

**Pré-condições.** Solicitante autenticado e autorizado a criar Usuário no Escopo em questão (RN-036, RN-038 a RN-041); e-mail informado não cadastrado na plataforma nem no provedor de identidade.

**Critério de aceitação.** Ao criar um Usuário com nome e e-mail válidos, ele passa a existir na plataforma guardando a referência ao identificador da sua identidade no provedor de identidade, na situação Pendente, sem senha própria (a identidade nasce sem credencial de acesso utilizável pelo Usuário — a senha é definida por ele no primeiro acesso), e recebe um Convite para o primeiro acesso (RN-035). Em nenhum momento existe Usuário na plataforma sem identidade correspondente no provedor de identidade.

**Nome de usuário no provedor de identidade.** A identidade nasce com nome de usuário derivado do e-mail, no formato aceito pelo provedor (apenas letras, números e underline): prefixo do ambiente (ex.: `dev_insp`) seguido de underline e do e-mail com todo caractere não alfanumérico substituído por underline; o resultado é limitado a 39 caracteres, não termina em underline (underline final vira `0`) e fica em minúsculas. O e-mail permanece o identificador de negócio; o nome de usuário é detalhe de integração e não é exibido na plataforma. Mesma derivação usada no InsurePoint legado.

**Casos limite.** Nome ou e-mail ausentes ou inválidos: criação recusada. E-mail já cadastrado na plataforma: criação recusada. E-mail já existente no provedor de identidade: criação recusada — a identidade preexistente não é adotada nem alterada. Falha ao registrar o Usuário na plataforma após a identidade ter sido criada no provedor: a identidade recém-criada é desfeita e a criação é recusada, sem deixar identidade órfã. E-mails distintos podem colidir no nome de usuário após o corte de 39 caracteres: o provedor exige nome de usuário único e recusa a criação — a criação é recusada sem identidade órfã, como em qualquer recusa do provedor.

## RN-002 — Ativação do Usuário no primeiro acesso

> Revisão proposta em 2026-07-23 (grill-rn, jornada Perfis e Permissões / [OPEN-06](../open-decisions.md)): a ativação passa a ocorrer pelo link de Convite (RN-035), sem senha inicial padrão nem autenticação prévia. Aguardando ratificação da PO — o texto abaixo já reflete a proposta.

**Descrição.** O Usuário na situação Pendente torna-se Ativo ao concluir o primeiro acesso pelo link de Convite (RN-035), definindo uma senha própria.

**Pré-condições.** Usuário na situação Pendente, portador de um Convite com link válido (RN-035).

**Critério de aceitação.** Ao abrir o link de Convite válido e definir a própria senha, a situação do Usuário passa de Pendente para Ativo. Enquanto o primeiro acesso não for concluído, o Usuário permanece Pendente e não acessa a plataforma (RN-005).

**Casos limite.** Link de Convite usado, expirado ou substituído por reenvio: primeiro acesso recusado, orientando solicitar novo Convite (RN-035); o Usuário permanece Pendente. Tentativa de acesso sem concluir o primeiro acesso: recusada, o Usuário permanece Pendente.

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
