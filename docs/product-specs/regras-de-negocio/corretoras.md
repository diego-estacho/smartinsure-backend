# Jornada: Corretoras

## RN-018 — Listagem de Corretoras

> Revisada em 2026-07-25 (redesign do CRUD, exec-plan 0009) — busca, filtros combinados, contagem por situação e paginação no servidor. Ratificada por Diego Estácho no lugar da PO (registrar confirmação da PO).

**Descrição.** A plataforma lista como Corretoras todas as Pessoas jurídicas que possuem Papel da Pessoa de corretor. A lista é paginada e ordenada pelo servidor (a base pode passar de dez mil Corretoras) e combina, por E lógico, busca livre e filtros: situação apresentada (Ativa, Incompleta ou Inativa — RN-053), Seguradora habilitada, Motor de Cálculo, setor (público ou privado, pela Natureza Jurídica) e período de cadastro (data em que o Papel da Pessoa de corretor foi criado). A busca livre casa por CNPJ (somente dígitos), razão social e nome fantasia.

**Pré-condições.** Usuário autenticado na plataforma; Pessoas jurídicas com Papel da Pessoa de corretor cadastradas.

**Critério de aceitação.** Sem filtros, o resultado contém Corretoras em qualquer situação. Cada filtro informado restringe o resultado e todos os filtros valem em conjunto. A plataforma devolve, além da página pedida, o total de resultados e a contagem de Corretoras por situação (Todas, Ativas, Incompletas, Inativas) considerando os demais filtros aplicados. Pessoa física e Pessoa jurídica sem Papel da Pessoa de corretor não aparecem na lista. Cada Corretora traz sua situação apresentada, a data de cadastro, as Seguradoras habilitadas e o Motor de Cálculo em uso.

**Casos limite.** Não havendo Corretoras para os filtros informados, a lista retorna vazia e as contagens vêm zeradas. Página além do total retorna vazia. Usuário não autenticado não acessa a lista.

## RN-019 — Criação de Corretora por CNPJ

> Revisada em 2026-07-25 (redesign do CRUD, exec-plan 0009) — a criação ocorre apenas na confirmação, carregando dados complementares e a escolha de ativação; a consulta prévia do CNPJ é somente leitura (RN-052). Ratificada por Diego Estácho no lugar da PO (registrar confirmação da PO).

**Descrição.** A plataforma cria uma Corretora a partir de um CNPJ válido no momento em que o usuário confirma o cadastro. Até a confirmação nada é gravado (a consulta do CNPJ é somente leitura — RN-052). Na confirmação, a plataforma garante que a Pessoa jurídica exista na base, adiciona o Papel da Pessoa de corretor e registra os dados complementares informados (nome fantasia, e-mail de contato, telefone e responsável — RN-054).

**Pré-condições.** Usuário autenticado informa um CNPJ válido de Pessoa jurídica, matriz ou filial, e confirma a criação.

**Critério de aceitação.** Se a Pessoa jurídica já existir sem Papel da Pessoa de corretor, a plataforma adiciona esse papel e devolve a Corretora criada. Se a Pessoa jurídica não existir, a plataforma a importa do Birô conforme RN-014 e adiciona o papel. A situação inicial segue a escolha do usuário: Ativa quando ele opta por ativar ao salvar, Inativa caso contrário. Os dados complementares informados são gravados junto; sem nome fantasia ou e-mail de contato, a Corretora Ativa é apresentada como Incompleta (RN-053).

**Casos limite.** CNPJ ausente, inválido ou documento de Pessoa física: criação recusada. CNPJ não localizado no Birô, Birô indisponível ou com erro: nada é criado. Pessoa jurídica que já possui Papel da Pessoa de corretor, em situação Ativa ou Inativa: criação recusada com notificação de Corretora já cadastrada, sem alterar a situação. Abandonar o cadastro antes de confirmar não deixa registro.

## RN-020 — Detalhes da Corretora

**Descrição.** A plataforma exibe os detalhes cadastrais de uma Corretora a partir dos dados da Pessoa jurídica vinculada ao Papel da Pessoa de corretor.

**Pré-condições.** Usuário autenticado consulta uma Corretora cadastrada.

**Critério de aceitação.** Ao abrir os detalhes da Corretora, a plataforma exibe CNPJ, nome, nome social, Natureza Jurídica, classificação de setor, endereço principal e situação da Corretora.

**Casos limite.** Corretora inexistente: consulta recusada com indicação clara. Usuário não autenticado não acessa os detalhes.

## RN-021 — Ativação e inativação de Corretora

**Descrição.** A plataforma permite ativar ou inativar uma Corretora alterando a situação do Papel da Pessoa de corretor, sem alterar a Pessoa nem seus demais papéis.

**Pré-condições.** Usuário autenticado solicita ativação ou inativação de uma Corretora cadastrada e confirma a operação.

**Critério de aceitação.** Ao confirmar a inativação de uma Corretora Ativa, sua situação passa a Inativa. Ao confirmar a ativação de uma Corretora Inativa, sua situação passa a Ativa. A alteração fica refletida na lista, no filtro de situação e nos detalhes da Corretora.

**Casos limite.** Cancelamento no diálogo de confirmação não altera a situação. Ativar Corretora já Ativa ou inativar Corretora já Inativa é recusado com indicação clara. Corretora inexistente: solicitação recusada com indicação clara. Nesta fase, a situação da Corretora não bloqueia automaticamente outros fluxos da plataforma.

## RN-052 — Consulta de CNPJ para cadastro de Corretora

> Catalogada em 2026-07-25 (redesign do CRUD, exec-plan 0009). Revisada em 2026-07-27 — a consulta passa a **persistir a Pessoa jurídica** retornada pelo Birô (sem o Papel da Pessoa de corretor) para reuso entre consultas, evitando custo de nova chamada; resolve a fatia de reuso da OPEN-04 (validade de 90 dias; Pessoas nunca confirmadas permanecem na base, sem limpeza automática nesta fase). Ratificada por Diego Estácho no lugar da PO (registrar confirmação da PO).

**Descrição.** No cadastro de uma Corretora, a plataforma consulta os dados de um CNPJ para o usuário revisar antes de confirmar. A consulta reaproveita os dados já cadastrados quando a Pessoa jurídica existir; quando não existir, obtém do Birô (RN-014) e **grava a Pessoa jurídica na base sem o Papel da Pessoa de corretor**. Persistir a Pessoa jurídica NÃO a torna Corretora nem a faz aparecer na listagem (RN-018), que considera apenas Pessoas com o Papel de corretor — a Corretora só é criada na confirmação (RN-019). O objetivo é evitar custo de nova chamada ao Birô numa consulta futura do mesmo CNPJ.

**Pré-condições.** Usuário autenticado informa um CNPJ válido de Pessoa jurídica durante o cadastro de Corretora.

**Critério de aceitação.** A plataforma devolve os dados cadastrais do CNPJ (razão social, nome fantasia, Natureza Jurídica, setor e endereço principal). Quando a Pessoa jurídica já existe na base, os dados vêm da base, sem consultar o Birô; a consulta ao Birô só ocorre quando a Pessoa jurídica não existe — gravando-a sem papel — ou quando ela existe sem o Papel de corretor e seus dados cadastrais têm mais de 90 dias, caso em que a plataforma reconsulta o Birô e devolve os dados atualizados apenas para exibição, sem alterar a Pessoa jurídica armazenada (import-once, RN-014) nem criar papel. Se o CNPJ já possuir o Papel da Pessoa de corretor, a plataforma sinaliza que a Corretora já está cadastrada e identifica o cadastro existente, sem alterá-lo nem reconsultar (import-once, RN-014). A consulta nunca cria nem altera o Papel da Pessoa de corretor nem a situação cadastral da Corretora, e não dispara efeito automático além do cache aqui descrito (os demais efeitos dos dados do Birô seguem em aberto na OPEN-04).

**Casos limite.** CNPJ ausente, inválido ou documento de Pessoa física: consulta recusada, nada é gravado. CNPJ não localizado no Birô, Birô indisponível ou com erro: a plataforma informa a falha e nada é gravado. Pessoa jurídica consultada e nunca confirmada como Corretora permanece na base sem papel — reutilizável em consultas futuras e invisível em todas as listagens (que filtram por papel); não há limpeza automática nesta fase. Reconsulta por validade vencida (mais de 90 dias) devolve os dados atualizados do Birô apenas para exibição, sem alterar a Pessoa jurídica armazenada nem criar Corretora; se o Birô falhar na reconsulta, a plataforma exibe os dados em cache.

## RN-053 — Situação apresentada da Corretora

> Catalogada em 2026-07-25 (redesign do CRUD, exec-plan 0009). Ratificada por Diego Estácho no lugar da PO (registrar confirmação da PO). Não cria status novo nem transição: a situação apresentada é derivada; a máquina de estados da Corretora segue Ativa/Inativa (glossário, ratificado 2026-07-17).

**Descrição.** Além do status armazenado do Papel da Pessoa de corretor (Ativo ou Inativo), a plataforma apresenta a Corretora em uma de três situações — Ativa, Incompleta ou Inativa — derivada no servidor a partir do status e da completude do cadastro. Considera-se o cadastro incompleto quando falta o nome fantasia ou o e-mail de contato.

**Pré-condições.** Corretora cadastrada.

**Critério de aceitação.** Corretora com papel Inativo é apresentada como Inativa, independentemente da completude. Corretora com papel Ativo e cadastro completo (nome fantasia e e-mail de contato presentes) é apresentada como Ativa. Corretora com papel Ativo e cadastro incompleto (sem nome fantasia ou sem e-mail de contato) é apresentada como Incompleta. A situação apresentada vale na listagem, na contagem por situação, no filtro e no detalhe, sempre calculada pelo servidor.

**Casos limite.** Completar o nome fantasia e o e-mail de contato de uma Corretora Incompleta a torna Ativa sem qualquer transição de status. Inativar uma Corretora Incompleta a torna Inativa; reativá-la volta a Incompleta enquanto o cadastro seguir incompleto.

## RN-054 — Edição de dados complementares da Corretora

> Catalogada em 2026-07-25 (redesign do CRUD, exec-plan 0009). Ratificada por Diego Estácho no lugar da PO (registrar confirmação da PO). Introduz os dados de contato da Corretora (e-mail, telefone, responsável) no glossário.

**Descrição.** A plataforma permite editar os dados complementares de uma Corretora — nome fantasia, e-mail de contato, telefone e responsável — sem alterar os dados obtidos do Birô (razão social, Natureza Jurídica e endereço principal), que seguem import-once (RN-014).

**Pré-condições.** Usuário autenticado edita uma Corretora cadastrada.

**Critério de aceitação.** Ao salvar, a plataforma grava os dados complementares informados e mantém inalterados os dados da Receita. Preencher ou remover nome fantasia e e-mail de contato reflete imediatamente na situação apresentada (RN-053).

**Casos limite.** Corretora inexistente: edição recusada com indicação clara. E-mail de contato em formato inválido: edição recusada com indicação clara. Campos complementares vazios são aceitos (a Corretora pode ficar Incompleta).

## RN-055 — Histórico da Corretora

> Catalogada em 2026-07-25 (redesign do CRUD, exec-plan 0009). Ratificada por Diego Estácho no lugar da PO (registrar confirmação da PO). Deriva da auditoria já existente; não introduz tabela de eventos.

**Descrição.** A plataforma apresenta o histórico de uma Corretora como uma linha do tempo dos eventos cadastrais registrados pela auditoria: a criação da Corretora, cada Habilitação de Seguradora (criação e mudança de situação) e a última edição de dados complementares. Cada evento traz data, hora e autor (o Usuário que realizou a ação, ou "sistema").

**Pré-condições.** Usuário autenticado consulta uma Corretora cadastrada.

**Critério de aceitação.** A plataforma devolve os eventos conhecidos da Corretora em ordem cronológica decrescente, cada um com tipo (criação, habilitação, alteração de situação de habilitação, edição de dados), descrição, data/hora e autor. A criação vem do vínculo do papel Corretor; as habilitações, de cada Habilitação de Seguradora; a edição, da última atualização dos dados complementares.

**Casos limite.** Corretora recém-criada sem habilitações mostra apenas o evento de criação. Como a auditoria guarda apenas a última atualização de cada registro, o histórico não reconstrói toda edição passada — apenas a mais recente de cada item (log evento-a-evento completo fica para uma tabela de auditoria dedicada, fora desta fase). Corretora inexistente: consulta recusada com indicação clara.
