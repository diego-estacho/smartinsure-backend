# Exec-plan 0002 — Criação e ativação de Usuário (RN-001, RN-002)

Status: em execução — backend e front implementados; PRs e E2E pendentes
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, RNs em `docs/product-specs/regras-de-negocio/usuarios.md`, glossário (termos Usuário e Provedor de identidade, ratificados 2026-07-15), OPEN-03 (vínculo Usuário↔Corretora — fora do escopo).

## Objetivo

Fatia vertical da jornada Usuários: criação de Usuário (RN-001) com identidade no provedor de identidade (Casdoor) e ativação no primeiro acesso (RN-002) — API no backend, tela de criação no front via BFF.

## Tarefas

- [x] RN-001/RN-002 catalogadas e aprovadas pela PO; glossário ratificado (Usuário, Provedor de identidade, Pendente/Ativo).
- [x] Core: `Usuario` (entidade rica), `ESituacaoUsuario`, `IUsuarioRepository`, `IProvedorIdentidade`.
- [x] Integration: cliente Casdoor (Refit + resiliência ADR-044), `CasdoorProvedorIdentidade`, options `SSO` (secret/senha default fora do versionamento).
- [x] Application: `CriarUsuarioUseCase` (Casdoor primeiro; compensação em falha local), `AtivarUsuarioUseCase`, validator.
- [x] Infra.Data: `UsuarioMapping` (e-mail e identidade externa únicos), `UsuarioRepository`.
- [x] Api: `UsuariosEndpoint` (POST /usuarios 201; POST /usuarios/ativacao — identidade do token, nunca do cliente).
- [x] Testes com rastreabilidade: `[Trait("RuleId", "RN-001")]` / `[Trait("RuleId", "RN-002")]` no backend; `describe('RN-001 ...')` no front.
- [x] Contrato `openapi.json` derivado e types do front gerados (openapi-typescript).
- [x] Front: BFF `server/api/usuarios.post.ts`, composable `useUsuarios`, página `/usuarios/novo` (validação de forma apenas).
- [x] Migration da tabela `Users` no repositório `smartinsure-dbmigration`: `V20260715114410__criar-tabela-users.sql` (branch `criacao-usuario`).
- [x] ADR-058 (artefatos em inglês, docs/UI pt-BR): código de domínio renomeado no mesmo PR (`User`, `EUserStatus`, rota `api/v1/users`, tabela `Users`); glossário com coluna de nome técnico; AGENTS.md/FRONTEND.md atualizados.
- [ ] Validar a migration localmente (`docker compose --profile migrations up -d` no backend — Docker estava desligado na criação).
- [ ] E2E Playwright da jornada (depende do dev-auth ADR-009 do front, ainda não implementado).
- [ ] PRs linkados pelo `AB#NNNNN` (PBI ainda não criado) — backend primeiro, depois front.

## Critérios de aceite

- `dotnet build` e `dotnet test` verdes; testes de arquitetura passando.
- `python scripts/check-harness.py` verde nos dois repos.
- Lint, typecheck e testes unitários verdes no front.
- Teste de RN carrega o ID (gate de rastreabilidade).

## Evidências

- Backend: `dotnet test` — 38/38 aprovados (inclui 11 testes novos RN-001/RN-002); `check-harness.py` → `harness ok`.
- Front: `pnpm test` — 4/4 aprovados; `pnpm lint` sem violações; `pnpm typecheck` sem erros; `check-harness.py` → `harness ok`.
- Contrato: `docs/generated/openapi.json` com `/api/v1/usuarios` e `/api/v1/usuarios/ativacao`, respostas tipadas (`Produces<T>`); `app/types/gen/api.ts` regenerado do contrato.
- Pendências registradas nas tarefas: migration no `DBMigrations`, E2E (bloqueado pelo dev-auth), PBI/PRs.
