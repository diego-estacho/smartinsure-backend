# Jornada: Usuários

## RN-001 — Criação de Usuário

**Descrição.** Qualquer usuário autenticado pode criar um novo Usuário na plataforma informando nome e e-mail. O Usuário nasce com identidade registrada no provedor de identidade, com senha inicial padrão de troca obrigatória, e na situação Pendente.

**Pré-condições.** Solicitante autenticado na plataforma; e-mail informado não cadastrado na plataforma nem no provedor de identidade.

**Critério de aceitação.** Ao criar um Usuário com nome e e-mail válidos, ele passa a existir na plataforma guardando a referência ao identificador da sua identidade no provedor de identidade, na situação Pendente, com a senha inicial padrão e a troca de senha exigida no primeiro acesso. Em nenhum momento existe Usuário na plataforma sem identidade correspondente no provedor de identidade.

**Nome de usuário no provedor de identidade.** A identidade nasce com nome de usuário derivado do e-mail, no formato aceito pelo provedor (apenas letras, números e underline): prefixo do ambiente (ex.: `dev_insp`) seguido de underline e do e-mail com todo caractere não alfanumérico substituído por underline; o resultado é limitado a 39 caracteres, não termina em underline (underline final vira `0`) e fica em minúsculas. O e-mail permanece o identificador de negócio; o nome de usuário é detalhe de integração e não é exibido na plataforma. Mesma derivação usada no InsurePoint legado.

**Casos limite.** Nome ou e-mail ausentes ou inválidos: criação recusada. E-mail já cadastrado na plataforma: criação recusada. E-mail já existente no provedor de identidade: criação recusada — a identidade preexistente não é adotada nem alterada. Falha ao registrar o Usuário na plataforma após a identidade ter sido criada no provedor: a identidade recém-criada é desfeita e a criação é recusada, sem deixar identidade órfã. E-mails distintos podem colidir no nome de usuário após o corte de 39 caracteres: o provedor exige nome de usuário único e recusa a criação — a criação é recusada sem identidade órfã, como em qualquer recusa do provedor.

## RN-002 — Ativação do Usuário no primeiro acesso

**Descrição.** O Usuário na situação Pendente torna-se Ativo ao concluir o primeiro acesso, que exige a definição de uma senha própria em substituição à senha inicial padrão.

**Pré-condições.** Usuário na situação Pendente, portador da senha inicial padrão.

**Critério de aceitação.** Ao autenticar-se pela primeira vez e concluir a troca obrigatória de senha, a situação do Usuário passa de Pendente para Ativo. Enquanto a troca de senha não for concluída, o Usuário permanece Pendente.

**Casos limite.** Nova senha igual à senha inicial padrão: troca recusada e Usuário permanece Pendente. Autenticação realizada sem conclusão da troca de senha: o Usuário não acessa as funcionalidades da plataforma e permanece Pendente.

## RN-005 — Autenticação de Usuário com e-mail e senha

**Descrição.** O Usuário na situação Ativo acessa a plataforma informando e-mail e senha. As credenciais são validadas exclusivamente no provedor de identidade — a plataforma não guarda nem valida senhas — e, quando válidas, o Usuário recebe acesso autenticado com validade de 8 horas.

**Pré-condições.** Usuário na situação Ativo, existente na plataforma e com identidade correspondente no provedor de identidade.

**Critério de aceitação.** Ao informar e-mail e senha reconhecidos pelo provedor de identidade, o Usuário Ativo obtém acesso autenticado à plataforma, válido por 8 horas; vencido esse prazo, um novo acesso exige nova autenticação. A validação da senha ocorre somente no provedor de identidade.

**Casos limite.** E-mail ou senha incorretos: acesso recusado com uma única mensagem que não revela se o e-mail está cadastrado. Usuário na situação Pendente: acesso recusado — o primeiro acesso acontece pelo fluxo de convite ([OPEN-06](../open-decisions.md)). Credenciais aceitas pelo provedor de identidade, mas sem Usuário correspondente na plataforma: acesso recusado com a mesma mensagem de credenciais incorretas. Provedor de identidade indisponível: acesso recusado com mensagem de indisponibilidade, distinta da de credenciais incorretas; acessos autenticados já concedidos permanecem válidos até o fim das suas 8 horas. Bloqueio por tentativas repetidas de acesso: não há nesta fase ([OPEN-05](../open-decisions.md)).
