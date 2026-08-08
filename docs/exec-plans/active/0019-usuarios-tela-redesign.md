# Exec-plan 0019 — Tela de Usuários: redesenho (Fatia A — listagem + detalhe, read-model + ações existentes)

Status: em andamento — slug `usuarios-tela`, AB# pendente. Cross-repo (backend read-model + frontend UI); PRs linkados pelo mesmo `AB#` quando sair.
Contexto obrigatório: `AGENTS.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, `docs/QUALITY_SCORE.md`; RN-001/RN-002/RN-012 (`usuarios.md`), RN-062..RN-076 (`perfis-e-permissoes.md`), RN-065 (Convite: link com validade/reenvio), glossário (situação do Usuário `Pending/Active/Inactive`, Convite), [OPEN-01](../../product-specs/open-decisions.md), [OPEN-06](../../product-specs/open-decisions.md), [OPEN-19](../../product-specs/open-decisions.md). Handoff de design: `../smartinsure-frontend/prototipos/usuarios/handoff-usuarios/01-usuarios.md`.

## Objetivo

Redesenhar a tela de Usuários conforme o handoff, começando pela **Fatia A**: reconstruir a **listagem** e o **detalhe** no kit `Si`, plugar no front as ações que **já existem** no backend (reenviar convite — RN-065; inativar/reativar — RN-076) e **enriquecer os read-models de leitura** com os campos que o novo design mostra, incluindo a situação de exibição **"Convite expirado"** derivada da expiração do Convite. Incremento vertical verificável, sem migration, sem RN nova, sem alterar nenhuma escrita.

## Decisões de produto que regem esta atividade (dono, 2026-08-07)

- Escopo **Tomador permanece** na tela (diverge do §2 do handoff). Perfil segue **opcional** (§1 relaxada; alinha RN-001 + OPEN-03).
- **"Convite expirado"** é situação de **exibição** derivada de `Usuário Pendente` + `Convite` vencido (RN-065). O enum `EUserStatus` continua `Pending/Active/Inactive` (status por nome estável, ADR-031/ADR-004) — **não** se cria enum novo. Exposto via flag no read-model.
- CPF, editar usuário, reset de senha, último acesso e criação inline de perfil entram em **fatias posteriores** (B–F).

## Escopo e não-escopo (Fatia A)

- **No escopo (backend):**
  - Enriquecer `UserListItemResponse` e `GetUserResponse` (+ DTOs de repositório e handlers `ListUsers`/`GetUser`, `UserRepository`) com: `scope` do contexto do vínculo/perfil; `profileIsFixed` (origem Fixo/Customizado, de `Profile.IsFixed`); `link`/vínculo (nome da Corretora/Tomador; "SmartInsure" no Escopo Sistema, dos memberships RN-064); `invitedBy` (de `EntityBase.CreatedBy`); `invitedAt` (`Invitation.CreatedAt`), `inviteExpiresAt` (`Invitation.ExpiresAtUtc`) e o derivado **`inviteExpired`** (`status==Pending && Convite não consumido && agora>ExpiresAtUtc`).
  - `GET /users`: filtro de situação em **5 vias** (todos/ativo/pendente/expirado/inativo, onde `expirado`=Pending+vencido e `pendente`=Pending+não-vencido) e **contagens por situação** para as abas (campo no `PagedResponse` ou rota de summary). Busca cobre nome/e-mail/perfil/vínculo.
  - Testes de use case com `[Trait("RuleId", …)]` (RN-012, RN-062/RN-064, RN-065, RN-076) e publicar `docs/generated/openapi.json`.
- **No escopo (frontend):** BFF + `useUsers` para reenviar/inativar/reativar (endpoints já existem); `lib/status/users.ts` com situação de exibição (4 estados) e abas 5-vias; pill com dot e fix do `Tabs` **no kit** (ADR-022); reescrita de `pages/usuarios/index.vue` (tabela `table-layout:fixed`, drawer de filtros, 4 estados, cards mobile, faixa de convites pendentes, rodapé+paginação) e de `pages/usuarios/[id].vue` (§11); redesenho dos modais de convite/corretor-admin e confirmação de inativação (só apresentação).
- **Fora do escopo (com motivo):**
  - **CPF** (§8/§9/§11): schema novo no `User` → Fatia B.
  - **Editar usuário** e exceção do e-mail (§9): endpoint novo → Fatia C.
  - **Reset de senha** (§10): sem RN nem endpoint → Fatia D.
  - **Último acesso** (§5/§11): não é rastreado hoje → Fatia E (a coluna/ filtro entram junto).
  - **Criação inline de perfil** (§8 passo 2) e chips de área do detalhe: dependem do **modelo de áreas**, cujo dono é a sessão de Perfis de acesso → Fatia F. Até lá, o atalho e os chips **linkam para `/perfis`**.
  - **Casa do "Convidar corretor administrador"** (decisão aberta, §Pontos abertos #1 do handoff): mantido em Usuários no Escopo Sistema por ora.

## Tarefas

- [ ] Backend: DTOs/Responses enriquecidos (projeção `AsNoTracking`, join Invitation + memberships + Profile).
- [ ] Backend: `ListUsersUseCase`/repositório com filtro 5-vias + contagens; `GetUserUseCase` com campos novos.
- [ ] Backend: testes `[Trait("RuleId", …)]`; `dotnet build`/`dotnet test`; regenerar `openapi.json`.
- [ ] Front: `types:gen`; BFF `server/api/users/[id]/{invitations/resend,inactivate,reactivate}.post.ts` + `useUsers`.
- [ ] Front: `lib/status/users.ts` (display 4 estados + abas 5-vias); `SiChip` dot + fix `SiTabs` no skin + vitrine `/dev/ui`.
- [ ] Front: reescrever `index.vue` + `components/users/FiltersDrawer.vue` + cards mobile + 4 estados.
- [ ] Front: reescrever `[id].vue` (§11); redesenhar modais + confirmação de inativação.
- [ ] Front: `vitest` (≥80%), `typecheck`, `check-harness`; evidência desktop + mobile.

## Critérios de aceite

- `GET /users` devolve os campos novos por nome estável, filtro 5-vias correto (expirado = Pendente com Convite vencido) e contagens coerentes com o filtro; busca por nome/e-mail/perfil/vínculo. Nenhuma migration; nenhuma escrita alterada.
- `GET /users/{id}` traz escopo, origem do perfil, vínculo, convidado por e os dados do convite (enviado em / expira em / expirado).
- Front: listagem e detalhe fiéis ao handoff (medidos), reenviar/inativar/reativar funcionando ao vivo com toasts do §10; "Convite expirado" aparece para Pendente vencido; desktop e mobile (~390px) resolvidos.
- Erros vêm do backend (ProblemDetails) e são consumidos por `extractApiErrorMessage` — nunca mensagem genérica cravada.

## Evidências

- Backend read-model (2026-08-07): `UserListItemResponse`/`GetUserResponse`/`UserMembershipResponse` enriquecidos (scope, origem fixo/customizado, vínculo, dados do Convite + flag `InviteExpired`); envelope `ListUsersResponse` com `UserStatusCountsResponse` (contagens 5-vias); filtro de situação 5-vias em `UserRepository.ListAsync` (Expirado = Pendente com Convite ativo vencido, RN-065). "Convidado por" adiado (o `CreatedBy` do audit não resolve nome). `dotnet build` = 0 erros; `dotnet test` UserUseCases = 87/87; tradução de query (ToQueryString) = 2/2.
- Front — ações que já existem no backend (2026-08-07): rotas BFF `server/api/users/[id]/{invitations/resend,inactivate,reactivate}.post.ts` + `useUsers.resendInvitation/inactivateUser/reactivateUser`. `pnpm install` + `pnpm typecheck` = EXIT 0.
- Contrato (2026-08-07): API subida na worktree (`:5158`, Development; mssql/mongo via docker já de pé). `docs/generated/openapi.json` regenerado do `/openapi/v1.json` — convertido LF→CRLF para casar com o commitado (sem churn de EOL) e validado (JSON.parse). Front: `types:gen` + ajuste dos aliases (`ListUsersResponse`) em `useUsers` e `server/api/users*.ts`; `pnpm typecheck` = EXIT 0.
- Kit + listagem (2026-08-07): `SiChip` ganhou `dot` (kit + vitrine `/dev/ui`); fix do `SiTabs` (`overflow-y:hidden` na tablist) no skin; `lib/status/users` com `getUserDisplayStatus` (4 estados de exibição) + `userStatusTabs` (5-vias). `pages/usuarios/index.vue` reescrita (cabeçalho §3, abas+contagem, busca cinza, tabela `table-layout:fixed`, pill com dot, menu de ações reenviar/inativar/reativar, faixa de convites pendentes §6, 4 estados, cards mobile, confirmação de inativação §10, toasts). `typecheck` + `lint` = EXIT 0.
- Diferido nesta fatia: **drawer de filtros avançados** (§4) — depende de params de filtro no `ListUsers` (perfil/escopo/vínculo/data) + fontes de opção, que ainda não existem; vira sub-passo próprio.
- Detalhe (2026-08-07): `pages/usuarios/[id].vue` reescrita (§11) — `SiPageBack`, hero (avatar/pill com dot/ação por situação), grid 2-col, card Perfis de acesso (Sistema + Vínculos, com "Abrir perfil"; chips de área + N permissões ficam p/ Fatia F), Atividade (envio do convite/expiração; último acesso real p/ Fatia E), Dados do acesso (Vínculo/Escopo/Cadastrado em; CPF p/ B, "Convidado por" adiado), Inativar/Reativar (RN-076). `typecheck` + `lint` = EXIT 0.
- Verificação ao vivo (2026-08-07): API `:5158` + front `:3000`; login real (usuário de teste, System Administrator). **Listagem** carrega com dados reais — abas com contagem (Todos/Ativo 1, demais 0), perfil representativo ("Sistema · Fixo"), vínculo "SmartInsure", pill "Ativo" com dot; **fecha o risco da tradução EF da nova `ListUsers`**. **Detalhe** mostra o modelo multi-vínculo (perfil de Sistema + 2 vínculos de Corretora), Atividade e Dados do acesso. **Mobile ~390px** vira cards (§12). **0 erros de console.** `vitest` = 393/393; `typecheck` + `lint` = EXIT 0; backend `build` + 89 testes. Screenshots: `usuarios-listagem-desktop/mobile`, `usuarios-detalhe-desktop`.
- Não exercido ao vivo: ações mutantes (reenviar/inativar/reativar) — só existe o admin Ativo (reenviar exige Pendente; inativar o único admin é barrado por RN-076). Wired ao padrão BFF/composable já testado.
- Modais (2026-08-07): `CreateScopedUserDialog` repaginado (§8 — descrição, hint do perfil, atalho "Criar perfil de acesso" → /perfis até a Fatia F, estado "sem perfis" com bloco tracejado, nota no rodapé) preservando a lógica de escopo RN-068/069/070; `InviteBrokerageAdministratorDialog` repaginado (§15 — Nome/E-mail em grid, Corretoras como "adicionar" com chips removíveis, nota). Verificados ao vivo (renderizam fiéis); `typecheck` + `lint` = EXIT 0. Screenshots `usuarios-modal-novo`, `usuarios-modal-corretor-admin`.
- Drawer de filtros avançados (§4, 2026-08-07): backend — `UserListFilters` (record) no `ListUsers` com perfil/escopo/vínculo/data de cadastro, aplicados na query base (contagens respeitam); enum de filtro movido para `Core/Enumerators/EUserListStatusFilter.cs` (ADR-031, teste de convenção). Front — `components/users/FiltersDrawer.vue` (Perfil/Escopo/Vínculo/Data; "Último acesso" fica p/ Fatia E) + botão "Filtros avançados" com badge + chips removíveis + "Limpar filtros" no `index.vue`; opções via `useProfiles`/`useBrokerages`. Contrato regenerado (query params). Verificado ao vivo: Escopo=Tomador → 0 resultados (estado vazio-filtro + chip + contagens a 0), "Limpar filtros" restaura; **0 erros de console**. `dotnet test` = 773/773; front `typecheck`/`lint`/`vitest` = EXIT 0 / EXIT 0 / 393. Screenshot `usuarios-drawer-filtro`.
- **Fatia A concluída e verificada ao vivo.** Próximo: Fatias B (CPF), C (editar), D (reset senha), E (último acesso), F (criação inline de perfil — consome a sessão de Perfis). Sem commit ainda (aguardando o dono).
