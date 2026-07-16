# Jornada: Cadastro de Pessoas

## RN-013 — Busca de Pessoa por nome ou documento

**Descrição.** A plataforma localiza Pessoas cadastradas a partir de um trecho do nome (razão social ou nome fantasia) ou de um CNPJ, devolvendo sempre uma lista. Pessoa já cadastrada é devolvida da própria base, sem consulta ao Birô e sem atualização de dados.

**Pré-condições.** O solicitante informa o termo de busca (trecho de nome ou documento) e o papel do contexto da busca (segurado ou tomador).

**Critério de aceitação.** Busca por trecho de nome devolve todas as Pessoas cujo nome ou nome fantasia contém o trecho informado. Busca por CNPJ já cadastrado devolve a Pessoa correspondente, sem gerar consulta ao Birô. No contexto de tomador, o resultado contém apenas matrizes (RN-016).

**Casos limite.** Trecho de nome sem correspondência: lista vazia. Termo que não é um CNPJ (CPF ou qualquer valor que não tenha 14 dígitos) e sem correspondência na base: lista vazia, sem consulta ao Birô — pessoa física não é importada por este fluxo.

## RN-014 — Importação de Pessoa a partir do Birô

**Descrição.** Quando a busca é por um CNPJ ainda não cadastrado, a plataforma consulta o Birô, grava a Pessoa na base com os dados retornados e a devolve na lista da própria busca.

**Pré-condições.** Busca por CNPJ (14 dígitos) sem correspondente na base; consulta ao Birô conforme RN-003.

**Critério de aceitação.** Com o Birô localizando o CNPJ, a Pessoa passa a existir na base com razão social, nome fantasia, CNPJ, Natureza Jurídica e o endereço retornado gravado como endereço principal — e a busca devolve a lista contendo essa pessoa. A importação ocorre uma única vez: nova busca pelo mesmo CNPJ devolve o registro da base, sem nova consulta e sem atualização dos dados por este fluxo.

**Casos limite.** Birô indisponível, com erro ou sem localizar o CNPJ: nada é gravado e a busca devolve lista vazia com aviso de que o CNPJ não foi localizado na fonte (a falha segue a RN-004). Natureza Jurídica retornada não catalogada na tabela de referência: importação recusada com erro de negócio e nada é gravado.

## RN-015 — Classificação da Pessoa pela Natureza Jurídica

**Descrição.** Toda Pessoa é classificada como setor público ou setor privado a partir do código de Natureza Jurídica, conforme a tabela de referência oficial (CONCLA).

**Pré-condições.** Tabela de Naturezas Jurídicas carregada na plataforma como dado de referência.

**Critério de aceitação.** Pessoa importada com código presente na tabela recebe a classificação (pública ou privada) daquela Natureza Jurídica, e a classificação acompanha a pessoa nas consultas.

**Casos limite.** Código não catalogado impede a importação (RN-014). A tabela é mantida manualmente: código novo publicado pela Receita Federal entra por atualização do dado de referência, sem tela de gestão nesta fase.

## RN-016 — Tomador é sempre a matriz

**Descrição.** No contexto de tomador, apenas a matriz (ordem `/0001` do CNPJ) pode figurar como Pessoa. Quando o solicitante informa o CNPJ de uma filial, a plataforma resolve a matriz correspondente e a devolve com a filial informada pré-selecionada.

**Pré-condições.** Busca no contexto de tomador (RN-013).

**Critério de aceitação.** Busca por nome ou documento no contexto de tomador devolve apenas matrizes. Busca pelo CNPJ de uma filial devolve a matriz da mesma raiz de CNPJ — importada conforme RN-014 quando ainda não cadastrada — com a indicação da filial pré-selecionada.

**Casos limite.** Matriz não localizada nem na base nem no Birô: lista vazia com aviso de não localizado. Matriz já cadastrada: devolvida da base, sem nova consulta ao Birô, mantendo a filial pré-selecionada.
