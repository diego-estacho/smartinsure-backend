---
id: ADR-062
title: Importação de Tags e Cláusulas Particulares embutidas no objeto da modalidade, no ciclo de catálogo
status: accepted
tags: [dominio, integracoes]
applies-to: ["src/SmartInsure.Core/Entities/ImportedModalityTag.cs", "src/SmartInsure.Core/Entities/ImportedModalityParticularClause.cs", "src/SmartInsure.Application.UseCase/Services/ModalityImports/**", "src/SmartInsure.Integration/CalculationEngines/**", "src/SmartInsure.Functions/**"]
supersedes: []
evidence: []
---

# ADR-062: Importação de Tags e Cláusulas Particulares embutidas no objeto da modalidade, no ciclo de catálogo

## Status

Aceito (2026-07-23, AB#0004). Estende o ciclo de importação de catálogo do [ADR-061](061-modalidade-derivada-da-global-modality.md)/RN-034 com o passo de Tags e Cláusulas (RN-047/RN-048/RN-049).

## Contexto

A **Tag** (desenho do formulário da modalidade, `JsonTag`) e as **Cláusulas particulares** de uma modalidade são mantidas pela OnPoint e entregues **embutidas na resposta do objeto da modalidade** (`POST /GetModalityObject`), que é consultado **por modalidade** (recebe o `ModalityUniqueId`, que no produto é o `SourceId` da `ImportedModality`). Não há endpoint próprio para tag nem para cláusula. Diferente do catálogo (`GetGroupAndModalities`, uma chamada por Corretora que traz todas as Seguradoras/modalidades), o objeto da modalidade é **N chamadas — uma por modalidade**.

Duas escolhas difíceis de reverter: (1) onde esse passo roda (job próprio × dentro do ciclo de catálogo); (2) o modelo de dados da cópia local (relação com `ImportedModality`, chave das cláusulas).

## Decisão (normativa)

- **O passo de Tags/Cláusulas roda dentro do ciclo de importação de catálogo** (RN-034), logo após o upsert das Modalidades Importadas de cada Seguradora processada **com sucesso**, iterando suas `ImportedModality` **Ativas** e chamando `GetModalityObject(BrokerCnpj, ModalityUniqueId=SourceId)` pelo Motor de Cálculo resolvido na Habilitação. Não há job, scheduler nem cadência próprios; o disparo agendado e o sob demanda (`POST /modality-imports/run`, Administrador do Sistema) são os mesmos do catálogo.
- **Cópia local em duas entidades novas** (SQL Server/EF Core, migrations Flyway — EF Migrations proibidas, ADR-041):
  - `ImportedModalityTag` — **1:1** com `ImportedModality` (chave única por `ImportedModalityId`); guarda o `JsonTag`, o texto do objeto e o status Ativa/Inativa. Só é gravada/atualizada quando o objeto traz `jsonTag` preenchido; nunca sobrescreve com vazio (RN-047).
  - `ImportedModalityParticularClause` — **N** por `ImportedModality`; identidade pela chave **(`ImportedModalityId`, `ExternalId`)** — o `id` da cláusula na OnPoint; guarda nome, texto, `JsonTag` e status (RN-048).
- **Resiliência e preservação herdadas** (RN-038/RN-039, RN-049): a falha na consulta do objeto de uma modalidade é isolada (não desativa sua Tag/Cláusulas, não interrompe as demais); a inativação por reconciliação só ocorre após consulta **bem-sucedida**; nada é apagado — inativa e reativa; cada execução é auditada no sumário da importação (`ModalityImportSummary`).
- **Nova operação no contrato do Motor** (`ICalculationEngine.GetModalityObjectAsync`), traduzida do payload PlugV2 por ACL própria (ADR-045), no padrão do `GetGroupAndModalities` (client resolvido sob demanda, base URL/`application-key-v2` por Habilitação, resiliência do HttpClient nomeado, ADR-044).

## Consequências

O corretor sempre vê os campos vigentes de cada modalidade sem intervenção manual, e as cláusulas acompanham no mesmo ciclo. Custo: o ciclo passa a fazer N chamadas adicionais por Seguradora (uma por modalidade ativa) — aceitável por rodar em baixa frequência e fora de pico (cadência configurável, OPEN-10); se o volume crescer, otimização (paralelismo/limite) entra em demanda própria. O acoplamento ao ciclo de catálogo é intencional (a fonte entrega tudo junto); se um dia a OnPoint publicar um endpoint dedicado de tag, o passo pode ser extraído sem mudar o modelo de dados. Migrations forward-only. Renderização do formulário e validação dos valores preenchidos ficam fora (demanda própria).
