---
id: ADR-015
title: Validação do JWT por chave simétrica compartilhada com o IdP
status: accepted
tags: [seguranca]
applies-to: ["src/*.Api/**"]
supersedes: []
evidence: []
---

# ADR-015: Validação do JWT por chave simétrica compartilhada com o IdP

## Status

Aceito

## Decisão (normativa)

- O JWT Bearer DEVE ser validado com `Authority` apontando pro IdP (Casdoor/OIDC) e `IssuerSigningKey` simétrica (`SymmetricSecurityKey`) compartilhada com o IdP.
- A validação DEVE exigir issuer, audience, lifetime e assinatura (`ValidateIssuer/Audience/Lifetime/IssuerSigningKey = true`).
- O secret da chave DEVE ser configurado via Options (ADR-053) e fornecido por variável de ambiente (ADR-054); NUNCA versionado.
- `RoleClaimType` DEVE apontar para a claim de role usada pelas policies.

## Contexto

A validação por chave simétrica compartilhada é o modelo suportado pela integração atual com o Casdoor. A alternativa por metadata/JWKS (chaves assimétricas com rotação automática) foi considerada e não adotada nesta decisão.

## Consequências

Trade-off aceito: a rotação da chave é um procedimento manual coordenado entre IdP e API (troca de secret + redeploy). O secret é um segredo de alto valor — vazamento permite forjar tokens; a gestão dele segue a ADR-054 sem exceções.
