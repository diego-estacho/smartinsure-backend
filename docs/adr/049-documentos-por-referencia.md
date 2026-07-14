---
id: ADR-049
title: Documentos por referência no parceiro, sem blob storage próprio
status: accepted
tags: [integracoes, dados]
applies-to: ["src/*.Providers.*/**"]
supersedes: []
evidence: []
---

# ADR-049: Documentos por referência no parceiro, sem blob storage próprio

## Status

Aceito

## Decisão (normativa)

- Arquivos e documentos de negócio DEVEM viver no sistema do parceiro; a aplicação NUNCA mantém blob storage próprio.
- Upload DEVE ser repasse multipart via client do provider (streaming, sem persistir localmente).
- Download DEVE ser por URL fornecida pelo parceiro (a aplicação intermedia ou repassa a URL, conforme o caso de uso).
- O banco guarda apenas metadados/referências (identificadores, URLs); conteúdo binário NUNCA é persistido em SQL ou Mongo.
- Geração de documentos (PDF etc.) NUNCA acontece na aplicação; documentos vêm prontos do parceiro.

## Contexto

Os documentos do domínio são emitidos e custodiados pelo parceiro — replicá-los localmente criaria segunda cópia com problema de sincronização, custo de storage e responsabilidade de retenção. O modelo por referência mantém o parceiro como fonte única do binário.

## Consequências

Disponibilidade de documento depende da disponibilidade do parceiro (mitigada pela resiliência da ADR-044). A aplicação não carrega ciclo de vida de arquivos (retenção, backup, LGPD sobre binários) — permanece no custodiante.
