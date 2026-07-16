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

## RN-010 — Perfil Administrador do Sistema

**Descrição.** O Usuário pode ter o Perfil Administrador do Sistema, que autoriza as operações internas da plataforma (como manter o catálogo de Seguradoras). Usuário sem Perfil é usuário comum. Somente um Administrador do Sistema concede ou revoga o Perfil de outro Usuário; o primeiro Administrador do Sistema nasce por operação interna da equipe SmartInsure.

**Pré-condições.** Concedente autenticado com o perfil Administrador do Sistema; Usuário destinatário existente na plataforma.

**Critério de aceitação.** Ao conceder o Perfil a um Usuário, ele passa a poder executar as operações exclusivas do Perfil; ao revogar, deixa de poder executá-las imediatamente. Concessão ou revogação solicitada por Usuário sem o Perfil é recusada por falta de permissão.

**Casos limite.** Conceder o Perfil a quem já o tem, ou revogar de quem não o tem: solicitação recusada com indicação clara de que o Usuário já está na condição pedida. Revogação que deixaria a plataforma sem nenhum Administrador do Sistema: recusada. Usuário destinatário inexistente: recusada com indicação clara.
