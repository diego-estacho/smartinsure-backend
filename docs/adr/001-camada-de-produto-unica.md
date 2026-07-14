# ADR-001: Camada de produto única, compartilhada entre repositórios

## Status
Proposto (ratificação na Fase A)

## Contexto
O produto vive em dois repositórios de código (`smartinsure-backend` e `smartinsure-frontend`). Vocabulário, regras de negócio e decisões precisam ser os mesmos nos dois lados; se cada repo mantiver sua cópia, a IA de cada lado diverge da do outro.

Opções consideradas:
1. Duplicar a camada de produto (glossário, RNs, ADRs) em cada repo
2. Um terceiro repositório só de documentação
3. Fonte única no backend, referenciada pelo front via workspace

## Decisão
A camada de produto do harness (glossário, RNs, constituição, decisões abertas, ADRs) é **fonte única** e vive só no `smartinsure-backend`. O `smartinsure-frontend` a referencia por workspace lado a lado, em vez de duplicá-la.

A costura entre os repos é mecânica: o backend publica o contrato OpenAPI no CI, o front gera os types a partir dele e o CI falha em caso de drift.

## Consequências
- Um único lugar para vocabulário e regra — sem drift entre back e front.
- O front depende do backend clonado como irmão no workspace (ADR-002).
- Type de API nunca é mantido à mão no front; vem do contrato publicado.
- Rejeitados: duplicar o harness por repo; um terceiro repositório só de docs.
