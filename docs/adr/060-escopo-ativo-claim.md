---
id: ADR-060
title: Escopo ativo (Corretora/Tomador) carregado como claim
status: proposed
tags: [api, seguranca, dominio]
applies-to: ["src/*.Api/**", "src/*.Infra.CrossCutting/Identity/**"]
supersedes: []
evidence: []
---

# ADR-060: Escopo ativo (Corretora/Tomador) carregado como claim

## Status

Proposto (2026-07-23) — aguardando ratificação do dono de arquitetura. Direção decidida na condução da jornada (mecânica da [OPEN-11](../product-specs/open-decisions.md)); endereça a RN-034. Só vira Aceito com a ratificação; a fatia 1b (implementação) não começa antes disso.

## Decisão (normativa)

- A Corretora ativa e o Tomador ativo de um Usuário são carregados como **claims** do acesso autenticado, na mesma fonte única de identidade do ADR-014 (`IClaimsTransformation`), e lidos por extensão de claims tipada (ex.: `GetActiveBrokerageId()`), nunca por re-parse do token cru.
- As Permissões efetivas do momento são as do Perfil do Usuário no Escopo ativo (RN-034); policies de autorização usam exclusivamente as claims enriquecidas (ADR-013/ADR-014).
- Trocar o Escopo ativo **reemite o acesso** com a nova claim; a seleção só é aceita se o Escopo pertencer aos vínculos do Usuário (`UserBrokerageMembership`/`UserPolicyHolderMembership`). Trocar o Escopo ativo não altera vínculos nem Perfis (RN-034).
- Escopo ativo é contexto de sessão (1 por acesso), não estado de domínio: nenhuma tabela de sessão nova — o portador é a claim.

## Contexto

A RN-034 exige que, com o Usuário vinculado a mais de uma Corretora/Tomador, exista um Escopo ativo que determina as Permissões efetivas. O ADR-014 já define que ids/tenant/roles vêm de claims enriquecidas — carregar o Escopo ativo como claim mantém uma única fonte de identidade e reaproveita o gancho de tenant (`GetBrokerId()`), em vez de introduzir estado de sessão paralelo.

Opções consideradas: claim no token (escolhida — coerente com ADR-014); sessão no servidor (estado paralelo, invalidação e limpeza próprias); header por request (não persiste, cliente rastreia); híbrido. As três últimas criam uma segunda fonte de contexto, contra o ADR-014.

## Consequências

- Troca de Escopo ativo custa uma reemissão de acesso (aceitável — ação pontual do Usuário).
- A invalidação de cache de identidade na troca de Perfil (ADR-014) vale também para a claim de Escopo.
- **Em aberto (não decidido por este ADR):** qual o Escopo ativo padrão no primeiro acesso quando há mais de um vínculo (única vira ativa? seleção obrigatória?) — segue na [OPEN-11](../product-specs/open-decisions.md), com sabor de PO/UX.
- A implementação do carregamento/troca da claim é a fatia 1b da jornada; a fatia 1a entrega apenas os vínculos que a seleção valida.
