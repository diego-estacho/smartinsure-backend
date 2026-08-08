# Exec-plan 0019 — Perfis de acesso: redesenho da tela + contrato (cross-repo)

Status: não iniciado — **Fase A (portão de design) em revisão; Fases B/C bloqueadas por ratificação da PO**
Contexto obrigatório (ler antes de executar): `AGENTS.md` (backend e front), `docs/PLANS.md`, `docs/product-specs/glossario.md`, `docs/product-specs/regras-de-negocio/perfis-e-permissoes.md`, `docs/product-specs/open-decisions.md`, `docs/adr/` (ADR-001 cross-repo, ADR-058 idioma, ADR-065 escopo ativo), e no front `docs/design-system-map.md` + `docs/FRONTEND.md`. Handoff de design: `prototipos/perfis de acesso/handoff-perfis-acesso/01-perfis-acesso.md`.

> Atividade cross-repo (`AB#` a definir; slug provisório `perfis-acesso`). PRs de backend, `smartinsure-dbmigration` e frontend linkados pelo mesmo `AB#`. Exec-plan mora no backend (PLANS.md). Substitui a Fatia 1 mínima de Perfis do front (rota `/perfis`) por `/perfis-acesso`.

## Objetivo

Entregar a tela de **Perfis de acesso** do handoff: editor de permissões agrupado por área com controle de três estados (Sem acesso · Consultar · Operar) + cascata de dependências, listagem rica (tabela + drawer + 4 estados + abas por escopo), detalhe, **exclusão com migração de usuários** e mobile em cards. Boa parte depende de **contrato novo** — por isso o backend vai primeiro (ADR-001).

## Gap contrato × handoff (o que motiva o backend)

| Handoff | Hoje no contrato | Fase |
|---|---|---|
| Catálogo por **área** + **dependência** + nota (§2/§3) | `PermissionResponse = {Id, Code, Description, IsSystem}`; 27 códigos `dominio.acao` sem área/dep | RN-063 (rev.) + B |
| **Descrição** do perfil (editor/lista/detalhe) | `Profile` sem `Description` | RN-082 + migration + B |
| Filtro **Criado em** | `Profile.CreatedAt` já existe (EntityBase), não exposto | B (expor DTO) |
| Coluna/filtro **Usuários** + "Quem usa" | sem contagem/lista de usuários por perfil | B (read-model via RN-064) |
| **Exclusão com migração** (§9) | `DELETE` recusa se em uso (erro genérico) | RN-074 (rev.) + B |
| Badges por escopo (abas) | sem contagem por escopo | B (facets) |

**Descoberta importante (RN-063):** o catálogo v1 cobre só funcionalidades **já construídas**. As ações do handoff **sem código v1** — emitir/aprovar/cancelar cotação, área Apólices inteira, Relatórios, convidar Corretor Administrador, "trocar perfil de usuário" — **não serão inventadas**: entram no catálogo quando a funcionalidade nascer (RN-063). O editor mostra o que o catálogo declara. As áreas **Apólices** e **Administração da plataforma** nascem vazias em v1.

## Contrato-alvo (a implementar na Fase B — não é o `openapi.json` gerado)

- `PermissionResponse` += `Area` (chave estável, ex.: `quotations`, `policy-holders`), `DependsOn` (código de permissão ou null), `Note` (string ou null). Rótulo da área é texto de UI (pt-BR) e mora no front (status por nome estável, ADR-058).
- `ProfileListItemResponse` += `Description` (string?), `CreatedAt` (date), `UserCount` (int), `AreaCount` (int).
- `GetProfileResponse` += `Description`, `CreatedAt`, `LinkedUsers` (top N: `{Id, Name, Email}`), `LinkedUserCount`.
- `ScopedProfileBody` += `Description` (string?, opcional).
- `DELETE /api/v1/profiles/{id}` aceita `migrateToProfileId` (obrigatório quando `UserCount>0`; servidor valida mesmo escopo, reatribui e então exclui — RN-074 rev. + RN-075). Sem usuários, exclusão imediata.
- `GET /api/v1/profiles` devolve **facets de contagem por escopo** para os badges das abas.
- Fonte de `UserCount`/`LinkedUsers`: vínculo Usuário↔Escopo↔Perfil (RN-064) — **confirmar a tabela de vínculo no modelo** como primeira tarefa da Fase B.

## Tarefas

### Fase A — Portão de design (drafts para a PO) — sem código de produto
- [x] Exec-plan (este arquivo) com o contrato-alvo e o fatiamento.
- [x] RN-063 (revisão PROPOSTA): área + dependência no catálogo, com a tabela de mapeamento dos 27 códigos → área/`DependsOn`/nota.
- [x] RN-074 (revisão PROPOSTA): exclusão de perfil em uso exige perfil-destino do mesmo escopo (migração), substituindo a recusa genérica.
- [x] RN-082 (nova, PROPOSTA): Descrição do perfil.
- [x] open-decisions: OPEN-27 (escopo Tomador na tela de Usuários), OPEN-28 (teto de `profiles.manage`), OPEN-29 (auditoria/versionamento da edição de perfil).
- [ ] **Gate:** ratificação da PO (converter PROPOSTA → aprovado) antes da Fase B.

### Fase B — Backend + migrations (pós-ratificação; 1 PR por assunto)
- [x] `smartinsure-dbmigration` (branch `develop`): `V20260807100000__…colunas-description-area-dependson.sql` (ALTER Profiles.Description + Permissions.Area/DependsOn) e `V20260807100100__…seed-area-dependson-catalogo.sql` (backfill dos 28 códigos por `Code`, idempotente).
- [x] Entities/Mappings: `Profile.Description` (+ `SetDescription`, param em `Create*`), `Permission.Area/DependsOn` (+ params em `Create`); `Profile`/`Permission`Mapping alinhados (colunas nullable).
- [x] DTOs/UseCases: `PermissionResponse` (+Area/DependsOn); `ProfileListItemResponse`/`Dto` (+Description/CreatedAt/UserCount/AreaCount) via `GetUsageAsync` (batelada, sem N+1); `GetProfileResponse`/`Dto` (+Description/CreatedAt/LinkedUsers top-5/LinkedUserCount); `ScopedProfileBody`/Create/Update (+Description → `SetDescription`); `DeleteScopedProfileUseCase` + `migrateToProfileId` → `ReassignMembershipsAsync` (migra vínculos e exclui numa transação).
- [x] Endpoints (`ProfilesEndpoint`): `ScopedProfileBody.Description`; `DELETE /{id}?migrateToProfileId`.
- [x] Testes: `dotnet build` verde; `dotnet test ~ProfileUseCases` = **29/29** verdes, incl. `Delete_ComUsuarios_DeveMigrarParaODestinoEExcluir` e `..._DeveRecusarDestinoDeOutroDono` (ponto de permissão → review humano no PR).
- [x] **Regenerar `docs/generated/openapi.json`** — subi a API local (:5158), extraí `/openapi/v1.json` e fiz **inserção cirúrgica** só dos meus campos (PermissionResponse.area/dependsOn; ProfileListItemResponse.description/createdAt/userCount/areaCount; GetProfileResponse.description/createdAt/linkedUsers/linkedUserCount; novo `ProfileLinkedUserResponse`; ScopedProfileBody.description; DELETE `migrateToProfileId`), preservando CRLF (diff +109/−3, JSON válido). Não arrastei o drift do `main` (contrato commitado estava atrás). Colisão de :5158 com a worktree `usuarios-tela` contornada esperando ela liberar.
- [ ] **Deferido (tech-debt):** facets de contagem por escopo p/ os badges das abas — não implementado nesta fatia; front renderiza as abas sem número por ora. Ver tech-debt-tracker.
- Nota: `policies.issue` (RN-513) existe no `PermissionCodes` mas **não** no seed do `develop` (veio por outra frente no `main`); o backfill por `Code` é no-op onde a linha não existe — reconciliar seed ao integrar.

### Fase C — Frontend (pós-contrato publicado; fatias verticais)
- [x] Ponte contrato→front: worktree `smartinsure-frontend` (irmã), `pnpm install`, `types:gen` (leu o contrato regenerado) — `api.ts` traz `area`/`dependsOn`/`userCount`/`areaCount`/`linkedUsers`/`linkedUserCount`/`migrateToProfileId`. **Correção de drift:** o `openapi.json` commitado (#44) estava atrás do próprio código (faltavam os schemas de detalhe-cotacao que o front #50 consome) — regenerei o contrato **completo** do código atual (CRLF), destravando o typecheck. Reconciliar no rebase p/ origin/main (o diff encolhe p/ só os meus campos se main já tiver o catch-up).
- [x] **Coração (verificado):** `lib/permissions/catalog.ts` (agrupa o catálogo por área + rótulos/notas pt-BR) + `lib/permissions/rules.ts` (funções puras §12: `togglePerm`/`addPerm`/`removePerm` cascata, `setAreaLevel`, `levelOf`, `areaSummary`) + `tests/unit/lib/permissions/rules.spec.ts` (**12/12 verdes**). `pnpm typecheck` do projeto verde.
- [x] `components/ui/SiSegmented.vue` (3 estados) + vitrine `/dev/ui` + de-para (design-system-map); `components/permissions/Editor.vue` → auto-import `<PermissionsEditor>` (usa catalog+rules; compartilhável com o convite). **Validado AO VIVO** no `/dev/ui` (Playwright, dev server :3000): renderiza fiel ao handoff (níveis, "Personalizado · N de M", notas, dicas de dependência), cascata/`setAreaLevel` funcionam, **mobile 390px** com o cabeçalho quebrando certo (gotcha flex-wrap+min-width resolvido), 0 erros de console; `pnpm typecheck` verde.
- [x] `pages/perfis-acesso/index.vue` (listagem: abas por escopo `[role=tablist]{overflow-y:hidden}`, busca, drawer, **4 estados**, tabela `table-layout:fixed` com larguras do §6, cards no mobile ≥44px) + `components/profiles-access/EditorDialog.vue` (modal, modos novo/duplicar/editar/editar-fixo) + `DeleteDialog.vue` (exclusão-com-migração §9) + `usePermissionsCatalog.ts`. `useProfiles.deleteProfile` + BFF DELETE aceitam `migrateToProfileId`.
- [x] `pages/perfis-acesso/[id].vue` (detalhe §10: hero, faixa de fixo, grid de auditoria marcadas/não-marcadas por área, "Quem usa", ações por origem).
- [x] Guarda de perfil fixo **na função** (`requestEdit`/`requestDelete` early-return; menu desabilita com tooltip explicando); gotchas de CSS aplicados.
- [x] Substituir a antiga: menu repointado (`shell.vue` → `/perfis-acesso`); `pages/perfis/*` e `components/profiles/FormDialog.vue` removidos; link "Abrir perfil" do detalhe de Usuário → `/perfis-acesso/{id}`.
- [x] **VALIDADO AO VIVO** (Playwright, backend :5158 + front :3000, login real diegoteste01, dados reais): listagem (6 perfis, userCount/areaCount reais, menu ativo), editor (catálogo real 27 perms, cascata "Operar", erro do backend na UI), DeleteDialog, detalhe (grid de auditoria), mobile 390px em cards. `pnpm typecheck` verde (0 erros); 12/12 testes de `rules.ts`.
- [x] **Bugs achados+corrigidos pela validação ao vivo (backend, pré-existentes, só expostos com catálogo semeado + perfil com permissão):** (1) `GET /profiles/{id}` 500 — `OrderBy` sobre record projetado no `Join` não traduz no EF → projeção anônima + ordenação em memória (`GetDetailsByIdAsync`); idem `LinkedUsers` (Union de Joins → 3 queries + merge). (2) `DELETE` 500 — apagar Perfil com `ProfilePermission` filho + FKs `Restrict` = relação obrigatória rompida → novo `IProfileRepository.RemoveWithPermissions` (apaga filhos antes do pai); testes atualizados. Após os fixes: `dotnet build` 0 erros, **29/29 testes**.
- [x] **Write round-trip validado ao vivo** (FINN ativa via switcher): **criar** perfil → toast "…criado. Ele já aparece na hora de convidar um usuário." + aparece na lista (1 permissão em 1 área) + refresh; **excluir** → toast "…excluído." + some + lista volta. UPDATE usa o mesmo PUT/composable (não exercido p/ não alterar dado real).
- **§13 (acoplamento Usuários):** entregue o lado desta atividade — `<PermissionsEditor>` compartilhável (a worktree `usuarios-tela` consome na fatia F, bloqueada neste modelo de áreas) + os links. **NÃO** toquei em `users/CreateScopedUserDialog.vue` (escopo da `usuarios-tela`; evita conflito de worktree).
- **Gaps documentados (limitações do contrato atual, não do design):** busca varre nome+descrição (não os rótulos de permissão — a lista não traz os códigos); filtro "Área com permissão" omitido (a lista traz `areaCount`, não o conjunto de áreas); badges de contagem por aba deferidos (TD-011). Registrar como follow-up de contrato.

## Critérios de aceite

- **A:** RNs/open-decisions revisáveis e coerentes com o glossário; `python scripts/check-harness.py` verde; nada de código de produto. Ratificação da PO é gate humano.
- **B:** `dotnet build` + testes verdes (≥80%); migration aplica no mssql local via flyway; `openapi.json` regenerado; teste da migração-na-exclusão.
- **C:** `pnpm install` + typecheck + `pnpm test` (inclui testes de `rules.ts` e do `PermissionsEditor`) + lint + build verdes; E2E/validação ao vivo (Playwright) desktop **e** 390px; screenshots dos 4 estados, editor (níveis + cascata + "Personalizado"), exclusão-com-migração e mobile em cards.

## Evidências

- **Backend:** `dotnet build` 0 erros; `dotnet test ~ProfileUseCases` **29/29** verdes (inclui migração-na-exclusão). Migrations aplicadas no mssql local (`v20260808140100`). Contrato regenerado (`/openapi/v1.json` → inserção cirúrgica) → `types:gen` traz os campos novos.
- **Front:** `pnpm typecheck` **0 erros**; `vitest rules.spec.ts` **12/12**.
- **Ao vivo (Playwright, backend :5158 + front :3000, login real, dados reais):** listagem (6 perfis, userCount/areaCount reais, menu ativo); editor com catálogo real (27 perms, cascata "Operar"/"Consultar", erro do backend na UI); **criar** perfil (toast + refresh); **excluir** perfil (toast + refresh, volta a 6); detalhe (grid de auditoria marcadas/não-marcadas, "Quem usa"); mobile 390px em cards (cabeçalho não colapsa). 0 erros de console (exceto o 422 esperado do guard de escopo ativo). Screenshots: `perfis-acesso-desktop`, `perfis-editor-modal`, `perfis-delete-dialog`, `perfis-detalhe`, `perfis-mobile`, `perfis-create-ok`, `perfis-delete-ok` (área de trabalho do dev).
- **2 bugs de backend pré-existentes achados+corrigidos** pela validação ao vivo (ver Fase C).
- Pendente: `git pull` + commit + 3 PRs linkados por `AB#`; ratificação da PO das RNs (Fase A).
