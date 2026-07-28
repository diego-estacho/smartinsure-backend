# Jornada: Tomadores

Cada RN é uma seção com o ID no título e os quatro blocos abaixo. O ID é `RN-NNN` numa **sequência única do catálogo** (não reinicia por jornada), estável e nunca reaproveitado. Linguagem de negócio, termos do [glossário](../glossario.md), sem path de código no corpo.

## RN-025 — Listagem e detalhes de Tomadores

**Descrição.** A plataforma lista as Pessoas com papel de tomador e apresenta os detalhes de cada uma. A entrada de um novo Tomador reaproveita o fluxo de busca de Pessoa (RN-013, RN-014, RN-016): criar Tomador é criar o vínculo de papel (RN-017), nunca um cadastro paralelo.

**Pré-condições.** Usuário autenticado na plataforma (nesta fase, sem restrição de Perfil).

**Critério de aceitação.** A listagem devolve todas as Pessoas com papel de tomador, com nome, nome social e documento, e permite localizar por trecho de nome ou documento. Os detalhes do Tomador apresentam os dados cadastrais da Pessoa, os endereços (RN-026), as Nomeações de Tomador (RN-027) e as Filiais (RN-052), com o cadastro de nova Filial disponível na própria ficha. A inclusão de um Tomador passa pela busca de Pessoa no contexto de tomador, que devolve apenas matrizes e cria o vínculo de papel.

**Casos limite.** Pessoa sem papel de tomador não aparece na listagem, mesmo cadastrada com outros papéis. Dados cadastrais importados do Birô (nome, nome social, CNPJ, Natureza Jurídica) não são editáveis nas telas do Tomador — a edição limita-se aos endereços adicionais (RN-026) e aos dados complementares de contato.

## RN-026 — Endereços do Tomador

**Descrição.** O Tomador possui exatamente um endereço principal — o importado do Birô (RN-014) — e pode ter quantos endereços adicionais forem necessários.

**Pré-condições.** Tomador existente na plataforma com endereço principal importado do Birô.

**Critério de aceitação.** O usuário autenticado inclui, altera e remove endereços adicionais do Tomador. O endereço principal não é editável nem removível, e nenhum endereço adicional assume o lugar de principal. As consultas do Tomador devolvem o endereço principal identificado como tal, junto aos adicionais.

**Casos limite.** Remoção de endereço adicional não afeta os demais nem o principal. Tomador sem endereços adicionais permanece válido apenas com o principal.

## RN-027 — Nomeação de Tomador

**Descrição.** A Nomeação de Tomador vincula uma Corretora a um Tomador junto a uma Seguradora. Um Tomador não pode ter mais de uma Nomeação Vigente para a mesma Seguradora — a Corretora nomeada é única por Seguradora. A Nomeação independe da Habilitação de Seguradora.

**Pré-condições.** Tomador existente na plataforma. Corretora Ativa e Seguradora Ativa nos respectivos cadastros. Usuário autenticado (nesta fase, sem restrição de Perfil).

**Critério de aceitação.** O usuário autenticado cria a Nomeação informando o Tomador, a Corretora e a Seguradora; ela nasce Vigente. Para cada par Tomador×Seguradora existe no máximo uma Nomeação Vigente. A criação não exige Habilitação de Seguradora entre a Corretora e a Seguradora. As Nomeações do Tomador aparecem nos detalhes do Tomador (RN-025), Vigentes e Encerradas.

**Casos limite.** Corretora ou Seguradora Inativa: criação recusada; Nomeações já existentes permanecem inalteradas. O mesmo Tomador pode ter Nomeações Vigentes com Corretoras diferentes, desde que para Seguradoras diferentes; e a mesma Corretora pode ser nomeada para várias Seguradoras do mesmo Tomador.

## RN-028 — Substituição e encerramento da Nomeação de Tomador

**Descrição.** A Nomeação Vigente termina por encerramento avulso ou por substituição — quando outra Corretora é nomeada para o mesmo par Tomador×Seguradora. Toda Nomeação terminada permanece no histórico como Encerrada; nada é excluído.

**Pré-condições.** Nomeação Vigente do par Tomador×Seguradora.

**Critério de aceitação.** Ao nomear outra Corretora para um par Tomador×Seguradora que já possui Nomeação Vigente, a vigente passa a Encerrada e a nova nasce Vigente, na mesma operação. O usuário também encerra a Nomeação Vigente sem substituta, deixando o Tomador sem Corretora nomeada para aquela Seguradora. O histórico do Tomador apresenta as Nomeações Encerradas com o período em que vigoraram.

**Casos limite.** Substituição pela mesma Corretora já Vigente é recusada — não gera novo registro no histórico. Nomeação Encerrada nunca volta a Vigente: retomar a mesma Corretora cria uma nova Nomeação. Encerramento não é afetado pelo status da Corretora ou da Seguradora — Nomeação de Corretora ou Seguradora Inativa pode ser encerrada.

## RN-052 — Filiais do Tomador

**Descrição.** O Tomador — sempre a matriz (RN-016) — possui a lista das suas Filiais cadastradas na plataforma. A Filial é cadastrada a partir do seu CNPJ, com os dados vindos do Birô como qualquer Pessoa jurídica (RN-014), e nasce obrigatoriamente vinculada à matriz da mesma raiz de CNPJ — Filial sem matriz não existe.

**Pré-condições.** Usuário autenticado na plataforma (nesta fase, sem restrição de Perfil). CNPJ de estabelecimento informado, na busca de Pessoa em contexto de tomador (RN-013) ou na ficha do Tomador (RN-025).

**Critério de aceitação.** Informado o CNPJ de uma Filial, a plataforma resolve a matriz correspondente pela raiz do CNPJ, cadastra a matriz pelo Birô quando ela ainda não existir (RN-014), cadastra a Filial pelo Birô quando ela ainda não existir, e vincula a Filial à matriz. Matriz e Filial já cadastradas são devolvidas da base, sem nova consulta ao Birô. Pessoa jurídica já cadastrada cujo CNPJ pertence à raiz da matriz e que ainda não esteja vinculada passa a ser vinculada, sem consulta ao Birô. A Filial não recebe papel de tomador (RN-017): não aparece na listagem de Tomadores nem nas buscas em contexto de tomador, que continuam devolvendo apenas matrizes. As Filiais do Tomador são apresentadas na ficha do Tomador (RN-025) e na etapa de tomador do Grupo de Cotação (RN-053).

**Casos limite.** Matriz não localizada no Birô: nada é gravado e a operação devolve aviso de não localizado. Matriz localizada e Filial não localizada — ou falha do Birô na segunda consulta: a matriz permanece cadastrada e utilizável, a Filial não é criada, e a operação avisa que o CNPJ da Filial não foi localizado na fonte. CNPJ inválido é recusado antes de qualquer consulta. CNPJ de ordem `/0001` informado como Filial é recusado — a matriz não é Filial de si mesma. Os dados da Filial são importados uma única vez, como os da matriz (RN-014); não há atualização periódica nesta fase. Remoção ou desvínculo de Filial está fora desta fase ([OPEN-17](../open-decisions.md)).
