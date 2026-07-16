# Jornada: Seguradoras

## RN-005 — Criação de Seguradora no catálogo

**Descrição.** O Administrador do Sistema cadastra Seguradoras no catálogo da plataforma informando CNPJ, razão social e, opcionalmente, nome fantasia e o endereço do logotipo (imagem hospedada fora da plataforma). A situação inicial — Ativa ou Inativa — é informada no cadastro.

**Pré-condições.** Solicitante com perfil Administrador do Sistema; CNPJ informado não cadastrado no catálogo.

**Critério de aceitação.** Ao cadastrar uma Seguradora com CNPJ válido (dígitos verificadores corretos) e único no catálogo, razão social preenchida e situação inicial informada, ela passa a existir no catálogo exatamente com esses dados. Os dados são informados manualmente — nenhuma consulta ao Birô é realizada no cadastro.

**Casos limite.** CNPJ ausente, com dígitos verificadores inválidos ou já existente no catálogo: cadastro recusado. Razão social ausente: cadastro recusado. Situação inicial não informada: cadastro recusado. Nome fantasia e logotipo ausentes: cadastro aceito (são opcionais). Endereço de logotipo informado que não seja um endereço válido de internet: cadastro recusado.

## RN-006 — Alteração de dados cadastrais da Seguradora

**Descrição.** O Administrador do Sistema pode alterar os dados cadastrais de uma Seguradora do catálogo (CNPJ, razão social, nome fantasia, logotipo), mantidas as mesmas exigências do cadastro.

**Pré-condições.** Seguradora existente no catálogo; solicitante com perfil Administrador do Sistema.

**Critério de aceitação.** Ao alterar os dados de uma Seguradora, as novas informações substituem as anteriores e as exigências do cadastro permanecem valendo: CNPJ válido e único no catálogo e razão social preenchida.

**Casos limite.** Alteração para CNPJ já usado por outra Seguradora: recusada. Remoção do nome fantasia ou do logotipo: aceita (são opcionais). Seguradora inexistente: recusada com indicação clara. A situação (Ativa/Inativa) não muda por alteração cadastral — somente pela ativação/desativação (RN-007).

## RN-007 — Ativação e desativação de Seguradora

**Descrição.** O Administrador do Sistema ativa ou desativa Seguradoras do catálogo. Inativa significa fora de operação — a Seguradora permanece no catálogo e nunca é excluída.

**Pré-condições.** Seguradora existente no catálogo; solicitante com perfil Administrador do Sistema.

**Critério de aceitação.** Ao desativar uma Seguradora Ativa, ela passa a Inativa; ao ativar uma Inativa, ela passa a Ativa. Em ambos os casos ela permanece no catálogo com seus dados preservados. Não existe operação de exclusão de Seguradora.

**Casos limite.** Ativar uma Seguradora já Ativa ou desativar uma já Inativa: solicitação recusada com indicação clara de que ela já está na situação pedida. Seguradora inexistente: recusada com indicação clara.

## RN-008 — Consulta operacional exclui Seguradoras Inativas

**Descrição.** A consulta padrão do catálogo, usada pelos fluxos operacionais da plataforma, retorna somente Seguradoras Ativas. A visão completa do catálogo — incluindo Inativas — é exclusiva do Administrador do Sistema.

**Pré-condições.** Usuário autenticado consultando o catálogo de Seguradoras.

**Critério de aceitação.** A consulta padrão do catálogo retorna apenas Seguradoras Ativas. Quando o Administrador do Sistema solicita a visão completa, as Inativas aparecem identificadas pela situação, sempre pelo nome estável da situação (nunca por posição ou número).

**Casos limite.** Usuário sem o perfil solicitando a visão completa: recebe somente as Ativas (a solicitação de visão completa não surte efeito). Catálogo sem nenhuma Seguradora Ativa: a consulta padrão retorna vazia.

## RN-009 — Escrita no catálogo exclusiva do Administrador do Sistema

**Descrição.** Incluir Seguradora, alterar seus dados e ativá-la ou desativá-la são operações internas da equipe SmartInsure, exclusivas do Administrador do Sistema. Para os demais usuários da plataforma, o catálogo de Seguradoras é somente leitura.

**Pré-condições.** Usuário autenticado na plataforma.

**Critério de aceitação.** Toda operação de escrita no catálogo feita por Usuário com o perfil Administrador do Sistema é aceita (atendidas as demais regras do catálogo); a mesma operação feita por Usuário sem o perfil é recusada por falta de permissão, sem nenhum efeito no catálogo. Qualquer Usuário autenticado consulta o catálogo.

**Casos limite.** Usuário não autenticado: nenhum acesso ao catálogo, nem de leitura. Usuário que teve o perfil revogado: as tentativas de escrita seguintes à revogação são recusadas.
