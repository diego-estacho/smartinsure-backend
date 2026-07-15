# Jornada: Usuários

## RN-001 — Criação de Usuário

**Descrição.** Qualquer usuário autenticado pode criar um novo Usuário na plataforma informando nome e e-mail. O Usuário nasce com identidade registrada no provedor de identidade, com senha inicial padrão de troca obrigatória, e na situação Pendente.

**Pré-condições.** Solicitante autenticado na plataforma; e-mail informado não cadastrado na plataforma nem no provedor de identidade.

**Critério de aceitação.** Ao criar um Usuário com nome e e-mail válidos, ele passa a existir na plataforma guardando a referência ao identificador da sua identidade no provedor de identidade, na situação Pendente, com a senha inicial padrão e a troca de senha exigida no primeiro acesso. Em nenhum momento existe Usuário na plataforma sem identidade correspondente no provedor de identidade.

**Casos limite.** Nome ou e-mail ausentes ou inválidos: criação recusada. E-mail já cadastrado na plataforma: criação recusada. E-mail já existente no provedor de identidade: criação recusada — a identidade preexistente não é adotada nem alterada. Falha ao registrar o Usuário na plataforma após a identidade ter sido criada no provedor: a identidade recém-criada é desfeita e a criação é recusada, sem deixar identidade órfã.

## RN-002 — Ativação do Usuário no primeiro acesso

**Descrição.** O Usuário na situação Pendente torna-se Ativo ao concluir o primeiro acesso, que exige a definição de uma senha própria em substituição à senha inicial padrão.

**Pré-condições.** Usuário na situação Pendente, portador da senha inicial padrão.

**Critério de aceitação.** Ao autenticar-se pela primeira vez e concluir a troca obrigatória de senha, a situação do Usuário passa de Pendente para Ativo. Enquanto a troca de senha não for concluída, o Usuário permanece Pendente.

**Casos limite.** Nova senha igual à senha inicial padrão: troca recusada e Usuário permanece Pendente. Autenticação realizada sem conclusão da troca de senha: o Usuário não acessa as funcionalidades da plataforma e permanece Pendente.
