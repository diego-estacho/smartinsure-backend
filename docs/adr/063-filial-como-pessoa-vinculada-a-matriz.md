---
id: ADR-063
title: Filial como Pessoa vinculada à matriz, e estabelecimento cotado no Grupo de Cotação
status: accepted
tags: [dominio, integracoes]
applies-to: ["src/SmartInsure.Core/Entities/Person.cs", "src/SmartInsure.Core/Entities/QuotationGroup.cs", "src/SmartInsure.Application.UseCase/UseCases/PersonUseCases/**", "src/SmartInsure.Application.UseCase/UseCases/PolicyHolderUseCases/**", "src/SmartInsure.Application.UseCase/UseCases/QuotationGroupUseCases/**"]
evidence: []
---

# ADR-063: Filial como Pessoa vinculada à matriz, e estabelecimento cotado no Grupo de Cotação

## Status

Aceito (2026-07-27, AB#0005). Formaliza a modelagem exigida pela RN-052 e pela RN-053.

## Contexto

Até aqui a Filial não existia como dado. A busca de Pessoa em contexto de tomador resolvia a matriz a
partir do CNPJ digitado (`CnpjValidator.HeadquartersOf`) e devolvia `PreSelectedBranchDocumentNumber` —
um campo **transitório da resposta**, que nada persistia e que o front sequer consumia. O Grupo de Cotação
não tinha onde registrar o estabelecimento, então não era possível cotar para uma Filial específica.

A demanda AB#0005 exige (RN-052) que a Filial seja cadastrada e **obrigatoriamente vinculada à matriz**, e
(RN-053) que o Grupo de Cotação registre qual estabelecimento é o objeto da garantia.

Duas modelagens foram consideradas: a Filial como **entidade própria** (tabela `PolicyHolderBranches`, com
CNPJ, nome e endereço próprios, análoga a `PersonAddress`), ou a Filial como **Pessoa jurídica** como
qualquer outra, com um vínculo para a matriz.

## Decisão (normativa)

- A **Filial é uma `Person`** (`Type = J`) como qualquer Pessoa jurídica, importada pelo **mesmo**
  `IPersonBureauImporter` da matriz (RN-014). Não existe entidade nem tabela própria de Filial: nome, nome
  social, Natureza Jurídica e endereço principal vêm do fluxo já existente, sem duplicar mapeamento do Birô,
  endereço nem unicidade de documento.
- O vínculo é **persistido, não derivado**: `Person.HeadquartersPersonId` (`Guid?`, auto-referência). A
  matriz tem `null`. Derivar a lista de Filiais por raiz de CNPJ foi descartado — exigiria busca por trecho
  de documento (sem índice útil) e transformaria qualquer PJ de mesma raiz em Filial por acidente, sem
  registrar intenção.
- Invariantes de domínio, verificadas na entidade: só Pessoa jurídica de ordem diferente de `/0001` pode ter
  matriz; a matriz apontada é a Pessoa jurídica `/0001` **da mesma raiz de 8 dígitos**; a matriz não é Filial
  de si mesma.
- A Filial **não recebe Papel da Pessoa** (RN-017). Consequência normativa: RN-016 permanece íntegra — a
  busca em contexto de tomador e a listagem de Tomadores continuam devolvendo apenas matrizes, e o **Tomador
  do Grupo de Cotação é sempre a matriz**.
- O Grupo de Cotação ganha `BranchPersonId` (`Guid?`, FK para `Persons`) — o estabelecimento cotado.
  **Ausente significa matriz**; não há valor sentinela nem linha de "estabelecimento matriz". A validação de
  que a Filial pertence à matriz do grupo é do servidor.
- Importação em cadeia (RN-052): CNPJ de Filial resolve a matriz, importa a matriz quando ausente, importa a
  Filial quando ausente e vincula. Falha do Birô na matriz não grava nada; falha na Filial **preserva a
  matriz** já cadastrada. A escolha é deliberada: descartar um cadastro de matriz válido — e pago, já que a
  consulta ao Birô tem custo por chamada (OPEN-04) — para punir a ausência da Filial seria pior.

## Consequências

O custo de implementação cai (zero lógica nova de importação) e a Filial nasce com o mesmo cadastro da
matriz, o que torna barato promovê-la a Segurado ou a Tomador se o negócio decidir isso depois. Em troca,
`Persons` passa a conter linhas que **não são Tomadores nem Corretoras nem Segurados** — Pessoas sem Papel,
visíveis a buscas por trecho de nome em outros contextos. Isso é aceitável: uma Filial *pode* legitimamente
ser segurada, e as listagens por papel (RN-018, RN-025) não são afetadas porque filtram por `PersonRole`.

A auto-referência em `Persons` cria um ciclo na própria tabela; o FK é `NO ACTION` e a exclusão de Pessoa
não existe na plataforma, então não há cascata a resolver.

Fica em aberto (OPEN-17) o efeito da Filial fora do Grupo de Cotação: qual CNPJ vai à Seguradora ao cotar
(etapa mockada, OPEN-07), Consulta de Crédito por Filial, Nomeação por estabelecimento e remoção/desvínculo.
