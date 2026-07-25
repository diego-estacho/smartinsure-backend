---
id: ADR-061
title: Catálogo de Modalidades — Modalidade derivada da Modalidade Global da OnPoint, vínculo intrínseco
status: accepted
tags: [dominio, integracoes]
applies-to: ["src/SmartInsure.Core/Entities/Modality.cs", "src/SmartInsure.Core/Entities/ImportedModality.cs", "src/SmartInsure.Application.UseCase/UseCases/Modality*/**", "src/SmartInsure.Application.UseCase/Services/ModalityImports/**", "src/SmartInsure.Functions/**"]
supersedes: ["ADR-060"]
evidence: []
---

# ADR-061: Catálogo de Modalidades — Modalidade derivada da Modalidade Global da OnPoint, vínculo intrínseco

## Status

Aceito. **Supersede o [ADR-060](060-catalogo-modalidades-dois-mundos.md)** após alinhamento com o time (2026-07-22).

## Contexto

O ADR-060 modelava dois mundos (Modalidade curada × Modalidade Importada) ligados por um **Mapeamento** próprio, com automação "por identificador do motor" e "por semelhança", curadoria e Fila de Revisão. Ao exercitar com dados reais do PlugV2 (dev, corretora Bravo), constatou-se: (1) a OnPoint **já entrega o vínculo** — cada Modalidade Importada carrega o id de uma **Modalidade Global** compartilhada entre Seguradoras, e **toda** importada tem esse id; (2) a granularidade da Modalidade é a da Modalidade Global (o caso "Judicial", OPEN-12, mostrou que tentar granularidade diferente da fonte gera mapeamento errado). Portanto, refazer o mapeamento do lado do Smart é desnecessário e propenso a erro — a fonte é a autoridade do vínculo.

## Decisão (normativa)

- A **Modalidade** (`Modality`) é derivada da **Modalidade Global** da OnPoint: a importação faz *find-or-create* por identificador global (`GlobalModalityExternalId` único) usando o nome da fonte. A Modalidade também pode ser **criada manualmente** pelo Administrador do Sistema (curadoria mantida); em ambos os casos o Administrador ativa/inativa (RN-039).
- **Não existe Grupo de Modalidade no lado Smart** — o antigo `ModalityGroup` é removido. O `ImportedGroup` (grupo da Seguradora) permanece como detalhe de origem.
- **Não existe entidade de Mapeamento** — o antigo `ModalityMapping` é removido. O vínculo `ImportedModality → Modality` é uma **referência direta**, resolvida automaticamente pelo id da Modalidade Global na importação; a origem do vínculo é registrada (`Automatic` × `Manual`) e o **override manual é preservado na reimportação** (RN-035/RN-037).
- A automação "por semelhança" (nome) **não existe** (encerra a OPEN-08 no contexto de modalidades). A "por identificador" deixa de ser uma etapa de confirmação e passa a ser o próprio vínculo intrínseco.
- **Nada opera sem vínculo**, mas o vínculo vem por construção (id global): uma Modalidade é oferecida com ≥1 Modalidade Importada Ativa e não Ignorada vinculada (RN-036); disponibilidade por ramo derivada; PF/PJ fica em aberto (OPEN-11).
- A **Fila de Revisão** trata apenas exceções (Importada sem id de Modalidade Global — hoje inexistente, defensivo) e curadoria: ignorar/reativar e override manual (RN-037).
- **Preservação** (RN-039) e **resiliência por Seguradora** (RN-038) permanecem como no ADR-060. A importação segue como Azure Function idempotente com motor resolvido pela Habilitação (RN-034).

## Consequências

O catálogo fica mais simples e alinhado à fonte: sem curadoria obrigatória de mapeamento, sem Fila de mapeamento no fluxo normal, sem Grupo no Smart. Perde-se a liberdade de granularidade diferente da OnPoint — decisão consciente (a fonte é a autoridade; OPEN-12 resolvida nesse sentido). Em relação ao já entregue nas fatias 1–3 sob o ADR-060, isto **retrabalha**: remove `ModalityGroup` e `ModalityMapping` (e suas telas/endpoints), transforma a `Modality` em importada+curada e o vínculo em referência direta. Migrations são forward-only (novas migrations ajustam o schema; as antigas não são reescritas).
