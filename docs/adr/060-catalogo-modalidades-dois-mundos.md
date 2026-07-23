---
id: ADR-060
title: Catálogo de Modalidades — dois mundos ligados por mapeamento, importação idempotente pela Habilitação
status: superseded
superseded-by: ADR-061
tags: [dominio, integracoes]
applies-to: ["src/SmartInsure.Core/Entities/Modality*.cs", "src/SmartInsure.Core/Entities/ImportedModality.cs", "src/SmartInsure.Application.UseCase/UseCases/Modality*/**", "src/SmartInsure.Application.UseCase/Services/ModalityImports/**", "src/SmartInsure.Functions/**"]
supersedes: []
evidence: []
---

# ADR-060: Catálogo de Modalidades — dois mundos ligados por mapeamento, importação idempotente pela Habilitação

## Status

**Superseded pelo [ADR-061](061-modalidade-derivada-da-global-modality.md)** (2026-07-22). O modelo abaixo (Mapeamento próprio, curadoria de mapeamento, Grupo de Modalidade no Smart, automação por identificador/semelhança) foi substituído: a Modalidade passou a ser derivada da Modalidade Global da OnPoint, com vínculo intrínseco (ver ADR-061). Mantido como registro histórico.

## Decisão (normativa)

- O catálogo de Modalidades tem **dois lados que nunca se confundem**: a **Modalidade** (e o **Grupo de Modalidade**), curada pela equipe, propriedade do Smart, que é o que o corretor vê e o eixo de comparação; e a **Modalidade Importada** (e o **Grupo Importado**), trazida da Seguradora exatamente como exposta. A ligação entre os dois é o **Mapeamento de Modalidade** (`ModalityMapping`), e é ele que torna as ofertas comparáveis (RN-036).
- O catálogo é **curado, não descoberto**: a importação NUNCA cria Modalidade nem Grupo de Modalidade — só cria/atualiza o lado importado e mapeamentos automáticos. A Modalidade só nasce por decisão humana explícita (curadoria ou promoção de pendência), com escrita restrita ao Administrador do Sistema (RN-032), espelhando a autorização do catálogo de Seguradoras (RN-011).
- A **verdade do lado importado é da fonte** (RN-033): nome de origem, público-alvo, ramo, grupo importado e parâmetros comerciais refletem sempre a última importação bem-sucedida e não são editáveis à mão. A Modalidade Importada é reencontrada pelo **identificador de origem** entre importações.
- A **importação** roda como **Azure Functions timer trigger** periódico, percorre as Seguradoras com Habilitação Ativa e Motor de Cálculo configurado, com **uma chamada por Seguradora** (deduplicada por Seguradora), e resolve o motor e os parâmetros de conexão **pela Habilitação** (RN-023) — nunca fixos no código. O processamento por Seguradora é **idempotente** (upsert por identificador de origem) e mirror do padrão de serviço importador da Application (`PersonBureauImporter`), com escopo de DI por Seguradora e falha isolada (RN-038), espelhando o assíncrono resiliente do ADR-050.
- **Nada entra na operação sem mapeamento confirmado** (RN-036): Modalidade Importada com mapeamento pendente, ausente ou ignorado não é oferecida; a disponibilidade e a comparabilidade da Modalidade são **derivadas** das Modalidades Importadas ativas confirmadas, nunca digitadas.
- A **trava de ramo** vive só no lado importado: a Modalidade (Smart) não carrega Ramo; nenhum mapeamento (automático ou manual) cruza ramos.
- **Preservação** (RN-039): nada no catálogo é apagado; itens saem de operação por Inativação. A Modalidade Importada é inativada automaticamente quando some de uma importação bem-sucedida da Seguradora (RN-038); a exclusão física é proibida.
- Nesta fase o mapeamento automático confirma **somente "por identificador do motor"**; o mapeamento "por semelhança" está fora do escopo (open-decision OPEN-08) e não é implementado.

## Contexto

Várias Seguradoras oferecem a mesma modalidade de Seguro Garantia com nomes e parâmetros diferentes. Para o corretor comparar sem conhecer o vocabulário de cada Seguradora, o produto precisa de uma lista única e estável de Modalidades sobre a qual cota uma vez e compara entre todas. Isso exige separar o que é do Smart (curado, poucos, estáveis) do que é da Seguradora (importado, muitos, mutáveis), com um mapeamento explícito entre os dois. A fonte atual é o Motor de Cálculo PlugV2 (`POST /GetGroupAndModalities`), acessado pelos parâmetros de conexão da Habilitação — a mesma seam já usada por RN-022..RN-024.

## Consequências

O corretor sempre vê a Modalidade; nomes e parâmetros da Seguradora só afloram ao cotar. Adicionar Seguradora ou reprocessar catálogo não toca a curadoria. O custo é a dupla escrituração (lado Smart × lado importado) e um ponto humano — a Fila de Revisão — para o que a automação não resolve com segurança; como o conjunto de modalidades de garantia é pequeno e muda pouco, a fila converge rápido. O trabalho é entregue em três fatias verticais (curadoria → importação → Mapa/Fila), cada uma com exec-plan e PR próprios.
