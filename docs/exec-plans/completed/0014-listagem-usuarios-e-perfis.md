# Exec-plan 0014 — Leitura de Usuários e Perfis (listagem e detalhe, Administrador do Sistema)

Status: **concluído** (2026-07-30) — nasceu como fatia de leitura sobre as fatias 0/1a/2/3a/5-parcial e cresceu, a pedido do dono do produto, para as sete fatias do escopo dos cinco contextos (leitura, primeiro acesso, perfis fixos + catálogo, Escopo ativo, criação por CA, Perfis customizados, criação por TA e Permissões dos fixos). Slug `perfis-permissoes`, AB# pendente. PR ainda não aberto.
Contexto obrigatório: `AGENTS.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, `docs/QUALITY_SCORE.md`, RN-062/RN-063/RN-064/RN-072/RN-075/RN-076 (`regras-de-negocio/perfis-e-permissoes.md`), RN-001/RN-002/RN-012 (`usuarios.md`), glossário (Perfil, Escopo do Perfil, Permissão, Vínculo, situação do Usuário), [OPEN-17](../../product-specs/open-decisions.md), [OPEN-18](../../product-specs/open-decisions.md), [OPEN-19](../../product-specs/open-decisions.md), [OPEN-20](../../product-specs/open-decisions.md).

## Objetivo

Dar visibilidade ao que as fatias anteriores já criaram: a plataforma tem Usuários, Perfis e Vínculos persistidos, mas nenhum endpoint de leitura — só criação e ações. Sem `GET`, não existe tela de Usuários nem de Perfis (o front mantinha os dois itens de menu desabilitados por falta de rota). Esta fatia entrega leitura paginada e detalhe, restritos ao Administrador do Sistema.

## Escopo e não-escopo

- **No escopo:**
  - `GET /users` — listagem paginada (`PagedResponse`, ADR-012) com busca opcional por nome/e-mail e filtro por situação; cada item traz nome, e-mail, situação (nome estável) e o Perfil de Escopo System quando houver (RN-012).
  - `GET /users/{id}` — detalhe do Usuário: dados de identificação, situação, Perfil de Escopo System e os Vínculos de Corretora e de Tomador com o Perfil de cada um (RN-064) — leitura do que a fatia 1a persistiu.
  - `GET /profiles` — catálogo de Perfis existentes (nome, Escopo, fixo/customizado, quantidade de Permissões marcadas) com filtro opcional por Escopo (RN-062).
  - `GET /profiles/{id}` — detalhe do Perfil com as Permissões marcadas (RN-062/RN-063). O catálogo de Permissões nasceu vazio na fatia 0: a lista vem vazia e isso é o estado honesto, não um erro.
  - Autorização: as quatro rotas exigem `Policies.SystemAdministrator`.
- **Fora do escopo (bloqueado/prematuro, com motivo):**
  - **Visibilidade por Escopo ativo** (Corretor Administrador vê os Usuários da sua Corretora, Tomador Administrador os do seu Tomador): a resolução do Escopo ativo em request está aberta na [OPEN-19](../../product-specs/open-decisions.md) (ADR-065 proposto, não ratificado) e os query filters por Corretora ativa (ADR-035) dependem dela. Enquanto isso, leitura restrita ao Administrador do Sistema — mesmo precedente da RN-076 na fatia 5 (ator sem ambiguidade).
  - **RN-072** (visibilidade dos Perfis oferecidos na criação de Usuário): é regra de criação por ator/Escopo, acopla à fatia 4 e à [OPEN-17](../../product-specs/open-decisions.md). O `GET /profiles` desta fatia é catálogo administrativo, não a lista oferecida na criação.
  - **Edição** de Permissões de Perfil fixo (RN-073, + [OPEN-18](../../product-specs/open-decisions.md)), criação/edição/remoção de Perfil customizado (RN-074, sob [OPEN-17](../../product-specs/open-decisions.md)) e troca de Perfil do Usuário na Corretora/Tomador (RN-075): esta fatia é somente leitura.
  - **Perfis fixos Corretor e Tomador**: não são semeados nem nomeados aqui — nome técnico está na [OPEN-17](../../product-specs/open-decisions.md). A listagem devolve os Perfis que existem no banco, sem inventar entrada.
  - **Aplicação das migrations** em qualquer ambiente: segue o gate das fatias anteriores (ADR-041, aplicação por CI). Esta fatia não adiciona migration — só leitura de tabelas já mapeadas.

## Tarefas

- [x] `UserDtos`/`ProfileDtos` em `Core/Abstractions/Repositories/Dtos` (projeção direta, ADR-038).
- [x] `IUserRepository.ListAsync` + `GetDetailsByIdAsync` (join de Vínculos com Person e Profile, `AsNoTracking`).
- [x] `IProfileRepository.ListAsync` + `GetDetailsByIdAsync` (Permissões marcadas).
- [x] `ListUsersUseCase`, `GetUserUseCase`, `ListProfilesUseCase`, `GetProfileUseCase` (convenção de scanning ADR-021 — sem registro manual).
- [x] `UsersEndpoint`: `GET /` e `GET /{id}` com `RequireAuthorization(Policies.SystemAdministrator)`.
- [x] `ProfilesEndpoint` (CarterModule novo) com as duas rotas Admin-only.
- [x] Testes de use case com `[Trait("RuleId", ...)]` (RN-012, RN-062, RN-063, RN-064).
- [x] `dotnet build`, `dotnet test`, `check-harness.py`; publicar `docs/generated/openapi.json` para o front consumir.

## Critérios de aceite

- `GET /users` devolve `PagedResponse` com página/tamanho saneados (página mínima 1, tamanho limitado a 100), busca por nome ou e-mail e filtro por situação; situação sai pelo nome estável do enum (ADR-031), nunca por índice.
- `GET /users/{id}` devolve os Vínculos de Corretora e de Tomador com o nome do escopo e o nome do Perfil; Usuário inexistente → `NotFound`.
- `GET /profiles` devolve todos os Perfis com Escopo e a marca de fixo; filtro por Escopo aplicado quando informado; `GET /profiles/{id}` inexistente → `NotFound`.
- Perfil sem Permissão marcada devolve lista vazia (RN-062: Perfil sem Permissão é válido).
- Requisição sem o Perfil Administrador do Sistema é recusada pela policy (fail-closed, `docs/SECURITY.md`).
- Nenhuma migration nova; nenhum comportamento de escrita alterado.

## Acréscimo — Escopo ativo (RN-064, ADR-065), 2026-07-30

Pedido do dono do produto ("a corretora em que o CA está logado"), com a mecânica decidida na
[OPEN-19](../../product-specs/open-decisions.md): o Escopo ativo virou claim do acesso.

- `ActiveScope` + `ScopeClaimNames`; `IAccessTokenIssuer.IssueFor(user, activeScope)` emite
  `active_brokerage`/`active_policy_holder` (claim ausente = sem Escopo daquele tipo).
- `ICurrentUserAccessor.ActiveBrokerageId`/`ActivePolicyHolderId` — o Escopo é lido do acesso,
  nunca do corpo da requisição (SECURITY.md).
- `IActiveScopeResolver`: Escopo padrão no login (vínculo único vira ativo; com vários, nenhum) e
  validação do Escopo pedido na troca (sem vínculo → recusa).
- `GetCurrentUserContextUseCase` (`GET /me`) e `SwitchActiveScopeUseCase` (`POST /me/active-scope`,
  reemite o acesso) em `MeEndpoint` — autenticado, sem policy de Admin: falam só do próprio acesso.
- `AuthenticateUserUseCase` passou a emitir o acesso já com o Escopo padrão.

## Acréscimo — Criação de Usuário pelo Corretor Administrador (RN-068/RN-069/RN-072), 2026-07-30

- `IScopeAuthorization`/`ScopeAuthorization`: exige solicitante Ativo, Escopo ativo selecionado e
  Perfil de administração naquele Escopo. Corretor/Tomador Administrador são Perfis **por Vínculo**,
  então a conferência é de dado no use case — policy de rota só serve para o Administrador do Sistema.
- `IInvitedUserService`/`InvitedUserService`: criação de Usuário convidado (identidade + Usuário +
  Convite + Vínculos numa transação, compensação da identidade, e-mail pós-commit), compartilhado
  pelos fluxos novos. `CreateUser` e `InviteBrokerageAdministrator` seguem com o código próprio
  deles — o dono do produto decidiu não refatorar os existentes.
- `InvitePolicyHolderAdministratorUseCase` (RN-068): recusa quando o Tomador não tem Nomeação
  Vigente com a Corretora ativa como nomeada (`ExistsActiveForPolicyHolderAndBrokerageAsync`).
- `InviteBrokerageUserUseCase` (RN-069): recusa Perfil de outro Escopo, de outra Corretora, e o
  Perfil Corretor Administrador (quem o concede é o Administrador do Sistema, RN-066).
- `ListAssignableProfilesUseCase` (`GET /profiles/assignable`, RN-072): oferece ao CA os Perfis da
  Corretora ativa (menos CA) mais Tomador Administrador; ao TA, os do Tomador ativo (menos TA);
  a Usuário comum, lista vazia (RN-071 depende de Permissão, adiada).
- `GET /users` deixou de ser Admin-only: a visibilidade agora é por Escopo (RN-064) — Administrador
  do Sistema vê todos, CA vê os Usuários da Corretora ativa, TA os do Tomador ativo, e quem não
  administra Escopo algum é recusado.

## Acréscimo — Perfis customizados por Escopo (RN-069/RN-070/RN-074/RN-063), 2026-07-30

- `Profile.CreateForBrokerage`/`CreateForPolicyHolder`, `Rename` (recusa Perfil fixo) e
  `ReplacePermissions` (o que sai da lista deixa de valer).
- `CreateScopedProfileUseCase`, `UpdateScopedProfileUseCase`, `DeleteScopedProfileUseCase` — nome
  único **por Escopo**, Permissão fora do catálogo recusada (RN-063), Perfil fixo nunca editado nem
  removido pelo administrador de Escopo, Perfil de outro dono recusado, e remoção bloqueada
  enquanto houver Usuário com o Perfil (contagem soma Vínculos de Corretora, de Tomador e o Perfil
  de Escopo Sistema).
- `IScopeAuthorization.RequireScopeAdministratorAsync`: resolve o Escopo administrado (Corretora
  ativa tem precedência sobre Tomador ativo, porque é o contexto principal do produto).
- `GET /profiles` e `GET /profiles/{id}` deixaram de ser Admin-only e passaram a respeitar a
  RN-072 na gestão: CA/TA veem apenas o próprio Escopo, e os Perfis fixos de administração
  (Administrador do Sistema, CA, TA) **não existem** para eles — nem listados, nem por
  identificador (devolve NotFound, não "sem permissão").
- `GET /permissions` (novo, autenticado): catálogo fixo declarado (RN-063) — é a lista oferecida ao
  marcar Permissões. Sem escrita correspondente: ninguém cria Permissão por tela.
- Migration `V20260730060026`: TD-008 fechada — nome de Perfil único por Escopo via índices
  filtrados. **Aplicada no banco de dev**; exigiu `SET QUOTED_IDENTIFIER ON` (índice filtrado).

## Acréscimo — Usuário do Tomador ativo e Permissões dos Perfis fixos (RN-070/RN-073), 2026-07-30

- `InvitePolicyHolderUserUseCase` (`POST /users/policy-holder-users`, RN-070): o Tomador
  Administrador cria Usuários do Tomador ativo com o Perfil fixo Tomador ou um customizado daquele
  Tomador; recusa Perfil de outro Escopo, de outro Tomador e o próprio Tomador Administrador (esse
  é concedido pelo Corretor Administrador, RN-068).
- `UpdateFixedProfilePermissionsUseCase` (`PUT /profiles/{id}/permissions`, RN-073): exclusivo do
  Administrador do Sistema (policy de rota), edita **apenas** as Permissões do Perfil fixo — nome,
  Escopo e estrutura seguem imutáveis — e a mudança vale globalmente. Recusa Perfil customizado
  (esse é RN-074, do administrador do Escopo) e Permissão fora do catálogo (RN-063). O efeito de
  remover Permissão essencial à administração segue **não definido** ([OPEN-18]): a operação é
  registrada em log para dar rastro até a decisão existir.

## Evidências

- `rtk dotnet build` → 13 projetos, 0 erro. `rtk dotnet test` → **465/465** (13 novos: `ListUsersUseCaseTests`, `GetUserUseCaseTests`, `ListProfilesUseCaseTests`, `GetProfileUseCaseTests`, com `[Trait("RuleId", "RN-012"|"RN-062"|"RN-063"|"RN-064")]` nos que exercem regra — saneamento de página e recusa de filtro inválido ficam sem RuleId por serem contrato de consulta, não regra de negócio).
- `python scripts/check-harness.py` → `harness ok`.
- Contrato publicado: `docs/generated/openapi.json` regerado da API em execução (`/openapi/v1.json`), agora com `/api/v1/users`, `/api/v1/users/{id}`, `/api/v1/profiles`, `/api/v1/profiles/{id}` — o diff também traz as rotas das fatias 2/3a/5 (convite, `brokerage-administrators`, inativação/reativação), que ainda não constavam no arquivo versionado.
- Sem migration nova; nenhuma escrita alterada. As quatro rotas são `RequireAuthorization(Policies.SystemAdministrator)`.
- Escopo ativo (acréscimo de 2026-07-30): `rtk dotnet test` **478/478** — novos `[Trait("RuleId","RN-064")]` em `ActiveScopeResolverTests` (vínculo único vira ativo; com vários não escolhe; sem vínculo fica vazio; troca recusa Corretora/Tomador sem vínculo; sair do Escopo é permitido), `SwitchActiveScopeUseCaseTests` (reemite acesso; recusa Usuário não-Ativo; NotFound; propaga recusa do resolver) e `JwtAccessTokenIssuerTests` (claims de Escopo presentes/ausentes). Migrations de seed dos Perfis fixos Corretor/Tomador e do catálogo de 27 Permissões aplicadas no banco de dev.
- Criação pelo CA (acréscimo de 2026-07-30): `rtk dotnet test` **495/495** — `[Trait("RuleId","RN-068")]` (convida TA com vínculo de Tomador; recusa sem nomeação vigente; NotFound de Tomador; propaga recusa de autorização), `[Trait("RuleId","RN-069")]` (vincula à Corretora ativa; recusa Perfil de outro escopo/outra corretora/CA; NotFound de Perfil), `RN-070` no guard de Tomador Administrador e `RN-064` na listagem restrita ao Escopo.
- Perfis customizados (acréscimo de 2026-07-30): `rtk dotnet test` **510/510** — `ScopedProfileUseCasesTests` com `RN-069` (cria vinculado ao Escopo; recusa nome repetido), `RN-063` (recusa permissão fora do catálogo), `RN-062` (perfil sem permissão é válido), `RN-074` (renomeia e troca permissões; recusa Perfil de outra corretora; remove sem usuários; recusa em uso; recusa fixo) e `RN-073` (fixo não é editado pelo administrador de Escopo); `ListProfilesUseCaseTests`/`GetProfileUseCaseTests` com `RN-072` (fixos de administração invisíveis para CA, inclusive por identificador). Correção de ordem descoberta pelos testes: a recusa por "Perfil fixo" vem antes da checagem de Escopo, senão a mensagem devolvida seria "não pertence ao seu escopo" (Perfil fixo é global, sem dono).
- RN-070/RN-073 (acréscimo de 2026-07-30): `rtk dotnet test` **521/521** — `InvitePolicyHolderUserUseCaseTests` (`RN-070`: vincula ao Tomador ativo com Perfil fixo Tomador; aceita customizado do próprio Tomador; recusa customizado de outro Tomador, Perfil de outro Escopo e o Perfil Tomador Administrador; propaga recusa de autorização) e `UpdateFixedProfilePermissionsUseCaseTests` (`RN-073`: marca e desmarca no Perfil fixo; recusa Perfil customizado; `RN-063` recusa Permissão fora do catálogo; NotFound).
- Pendências: leitura por Escopo ativo ([OPEN-19](../../product-specs/open-decisions.md)); aplicação das migrations 0008/0009/0010 + seed 3a em banco de dev (gate herdado das fatias anteriores) — **sem isso as rotas de leitura não têm as tabelas `Profiles`/`Permissions`/`UserBrokerageMemberships` para consultar**; AB#/PBI; PR.
