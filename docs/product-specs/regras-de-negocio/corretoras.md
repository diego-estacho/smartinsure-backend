# Jornada: Corretoras

## RN-018 — Listagem de Corretoras

**Descrição.** A plataforma lista como Corretoras todas as Pessoas jurídicas que possuem Papel da Pessoa de corretor, permitindo filtrar pela situação Ativa ou Inativa desse papel.

**Pré-condições.** Usuário autenticado na plataforma; Pessoas jurídicas com Papel da Pessoa de corretor cadastradas.

**Critério de aceitação.** Ao consultar a lista sem filtro de situação, o resultado contém Corretoras Ativas e Inativas. Ao informar a situação Ativa ou Inativa, o resultado contém somente Corretoras naquela situação. Pessoa física e Pessoa jurídica sem Papel da Pessoa de corretor não aparecem na lista.

**Casos limite.** Não havendo Corretoras cadastradas para o filtro informado, a lista retorna vazia. Usuário não autenticado não acessa a lista.

## RN-019 — Criação de Corretora por CNPJ

**Descrição.** A plataforma cria uma Corretora a partir de um CNPJ válido, garantindo que a Pessoa jurídica exista na base e possua Papel da Pessoa de corretor com situação Ativa.

**Pré-condições.** Usuário autenticado informa um CNPJ válido de Pessoa jurídica, matriz ou filial.

**Critério de aceitação.** Se a Pessoa jurídica já existir sem Papel da Pessoa de corretor, a plataforma adiciona esse papel com situação Ativa e devolve a Corretora criada. Se a Pessoa jurídica não existir, a plataforma usa a busca de Pessoa por documento; com o Birô localizando o CNPJ, grava a Pessoa jurídica conforme RN-014, adiciona o Papel da Pessoa de corretor com situação Ativa e devolve a Corretora criada.

**Casos limite.** CNPJ ausente, inválido ou documento de Pessoa física: criação recusada. CNPJ não localizado no Birô, Birô indisponível ou com erro: nada é criado. Pessoa jurídica que já possui Papel da Pessoa de corretor, em situação Ativa ou Inativa: criação recusada com notificação de Corretora já cadastrada, sem alterar a situação.

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
