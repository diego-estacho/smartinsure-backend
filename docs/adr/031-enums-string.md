---
id: ADR-031
title: Enums com prefixo E, persistidos como string
status: accepted
tags: [dominio, dados]
applies-to: ["src/*.Core/Enumerators/**", "src/*.Infra.Data/Mappings/**"]
supersedes: []
evidence: []
---

# ADR-031: Enums com prefixo E, persistidos como string

## Status

Aceito

## Decisão (normativa)

- Enums de domínio DEVEM usar o prefixo `E` (ex.: `EStatusProposta`) e viver em `Core/Enumerators/`.
- Persistência DEVE ser como string via `HasConversion<string>()` no mapping Fluent; enums NUNCA são persistidos como int.
- Rótulos de apresentação (nomes amigáveis pt-BR) NUNCA vivem no enum do domínio — tradução para exibição acontece na borda (response/mapper).
- Remoção ou renomeio de membro de enum persistido DEVE ser tratado como mudança de dados (migration de conversão), nunca como rename silencioso.

## Contexto

Persistir como string torna o dado legível em consultas diretas ao banco e imune a reordenação de membros do enum — persistência por int quebra silenciosamente quando alguém insere um membro no meio. O prefixo `E` distingue enum de entidade homônima na leitura.

## Consequências

Colunas de status ocupam mais espaço que int (irrelevante no volume esperado) e ganham legibilidade operacional. Renomear membro exige migration de dados — atrito deliberado que protege dados históricos.
