# Exec-plan 0010 — Convite de Primeiro Acesso (RN-035, fatia 2)

Status: implementação — fatia 2 da jornada Perfis e Permissões (slug provisório `perfis-permissoes`, AB# pendente). Revisa RN-001/RN-002.
Contexto obrigatório: `AGENTS.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, RN-035 (`regras-de-negocio/perfis-e-negocio/perfis-e-permissoes.md`), IMailService (ADR-048), IIdentityProvider (RN-001/005), ADR-014 (identidade única).

## Objetivo

Implementar o fluxo de primeiro acesso por convite (RN-035): todo Usuário criado nasce Pendente + convite por e-mail com link de uso único (7 dias). Primeiro acesso pelo link → define a própria senha → ativa no Usuário. Reenviável enquanto Pendente (invalida anterior). Substitui a senha inicial padrão de hoje. Remove o caminho antigo de ativação (`ActivateUserUseCase` + POST /users/activation).

## Escopo

- **Novas entidades:** `Invitation` (uso único, persiste em tabela com hash do token, validade, reenviável).
- **Novas abstrações:** `IInvitationRepository` (GetByTokenHashAsync, GetPendingByUserAsync).
- **Novos usecases:** `AcceptInvitationUseCase` (primeiro acesso), `ResendInvitationUseCase` (reenvio).
- **Novos endpoints:** `POST /users/invitations/accept` (anônimo, token+senha), `POST /users/{id}/invitations/resend` (autenticado).
- **Edições:** `CreateUserUseCase` gera Invitation + envia e-mail; `IIdentityProvider.SetPasswordAsync` novo (relay admin ao Casdoor `/api/update-user`); `UsersEndpoint` remove POST /users/activation.
- **Config:** `InvitationOptions` (AppBaseUrl, LinkExpiryDays), Options Pattern ADR-053.

## Critérios de aceite

- Usuário criado = Pendente + Convite por e-mail com link uso-único.
- Token plaintext não logado; só hash na Invitation.
- Token aleatório forte (32 bytes) gerado com `RandomNumberGenerator.Create()`.
- Um convite ativo por Usuário (índice único filtrado `WHERE ConsumedAtUtc IS NULL`).
- Primeiro acesso: token valido→SetPassword(Casdoor)→marca Convite consumido→Activate(User).
- Reenvio invalida anterior (marca como consumido).
- Link expira em 7 dias (via config, não hardcode).
- Falha de e-mail não derruba CreateUser (User fica Pendente, Convite reenviável).

## Revisão de segurança obrigatória no PR (SECURITY.md — ponto de auth)

- **`SetPasswordAsync` via Casdoor `/api/update-user` (relay com credencial de app)**: a semântica exata desse endpoint NO DEPLOYMENT Casdoor precisa ser CONFIRMADA (o update sobrescreve só a senha? efeitos colaterais em outros campos do usuário? exige flag admin?). Decidido em sessão como o caminho consistente com o `/api/add-user` já usado, mas exige teste contra o Casdoor real e review humano de segurança antes do merge.
- Endpoint de aceite é `AllowAnonymous` (correto — primeiro acesso sem sessão); validar rate-limiting/abuso é pendência de OPEN-05 (login), não desta fatia.
- A plataforma nunca guarda a senha (RN-005): o aceite apenas relaya a senha escolhida ao provedor de identidade.

## Evidências

- Backend: `rtk dotnet build` 0 erros; `rtk dotnet test` **318/318** (novos com `[Trait("RuleId","RN-035")]`: Invitation, AcceptInvitation, ResendInvitation; CreateUser marcado RN-001+RN-035; `ModelBuildingTests` estendido com FK Invitation→User). `check-harness.py` → `harness ok`.
- Correções pós-fork (revisão): `CreateUserUseCase` passou a gravar Usuário+Convite em **commit único** e o envio de e-mail saiu do escopo de compensação de identidade (falha de e-mail não desfaz a criação — RN-035); migration da tabela `Invitations` alinhada ao bloco de auditoria do `EntityBase` (`CreatedAt/CreatedBy/UpdatedAt/UpdatedBy` — o fork havia criado `CreatedAtUtc`, drift que quebraria o insert) + mapping 1:1.
- Migrations: `V20260723164510__criar-tabela-invitations.sql` (hash do token, índice único filtrado por Usuário ativo). **NÃO aplicada** — aplicação (após 0008/0009, mesma ordem) gateia o PR.
- Correções pós-revisão (advisor): RN-002 (Pendente→Ativo) reganhou cobertura — `AcceptInvitationUseCaseTests` agora tem `[Trait("RuleId","RN-002")]` e asserta `Status == Active`; `ResendInvitationUseCase` grava o consume do Convite anterior ANTES de inserir o novo (o índice único filtrado de "um ativo por Usuário" proíbe dois ativos e o EF não garante UPDATE antes de INSERT no mesmo commit) — **confirmar no apply da migration**; RN-001 reconciliada ("sem senha própria — identidade sem credencial utilizável até o primeiro acesso") pra casar com o `CreateIdentityAsync`.
- Pendências: aplicar migration no banco (confirmar o comportamento do resend contra o índice único); confirmação + review de segurança do `update-user` Casdoor; ratificação PO do prazo de 7 dias (OPEN-06); AB#/PBI; PR.
