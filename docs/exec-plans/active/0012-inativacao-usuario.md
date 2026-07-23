# Exec-plan 0012 — Inativação/reativação de Usuário (RN-046, fatia 5 parcial)

Status: em andamento — parte buildável da fatia 5 (slug `perfis-permissoes`, AB# pendente). Sobre as fatias 0/1/2/3a.
Contexto obrigatório: `AGENTS.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, RN-046 e RN-043/044/045 (`regras-de-negocio/perfis-e-permissoes.md`), RN-005 (`usuarios.md`), glossário (status Usuário Inativo), [OPEN-12](../../product-specs/open-decisions.md).

## Objetivo

Entregar a parte da fatia 5 (gestão) que não depende de peças bloqueadas: inativação/reativação de Usuário (RN-046) restrita ao Administrador do Sistema. Adiciona o status Inativo e a recusa de login do Usuário Inativo.

## Escopo e não-escopo

- **No escopo:** status `Inactive` (transição Ativo↔Inativo, RN-046); `User.Deactivate()/Reactivate()`; `ChangeUserActivationUseCase` (Administrador do Sistema; guarda de não deixar a plataforma sem Administrador do Sistema); endpoints `POST /users/{id}/inactivate` e `/reactivate`; recusa de login do Inativo (RN-005/RN-046) com mensagem própria.
- **Fora do escopo (bloqueado/prematuro, com motivo):**
  - **RN-046 por escopo** (Corretor/Tomador Administrador, usuário comum com permissão) e o caso do Usuário multi-Corretora → [OPEN-12](../../product-specs/open-decisions.md) (semântica global vs por escopo) + enforcement por permissão (RN-033, adiado).
  - **RN-043** (editar permissões de Perfil fixo): não há catálogo de Permissões (a tabela nasceu vazia na fatia 0; códigos/enforcement são a fatia de enforcement) — não há permissão para marcar/desmarcar; + [OPEN-10].
  - **RN-044** (editar/remover Perfil customizado): Perfis customizados ainda não podem ser criados (fatia 4, sob OPEN-09).
  - **RN-045** (trocar Perfil do Usuário): os Perfis disponíveis e a autorização "gerenciar usuários" acoplam à fatia 4/enforcement.
  - **Guard de alvo da RN-046** para Corretora/Tomador ("não deixar o Escopo sem administrador"): implementado só para o Escopo System (último Administrador do Sistema). Limitação consciente: o Administrador do Sistema pode atualmente inativar o último Corretor/Tomador Administrador de um Escopo — o guard equivalente para CA/TA precisa de contagem de administradores ativos por Escopo e acopla à semântica de escopo → diferido em [OPEN-12](../../product-specs/open-decisions.md).

## Tarefas

- [x] `EUserStatus.Inactive`; glossário com o status e as transições (já proposto na fatia 0).
- [x] `User.Deactivate()`/`Reactivate()` (transições Ativo↔Inativo, idempotência recusada por conflito).
- [x] `ChangeUserActivationUseCase` (Admin; guarda do último Administrador do Sistema).
- [x] Endpoints `inactivate`/`reactivate` com `RequireAuthorization(Policies.SystemAdministrator)`.
- [x] `AuthenticateUserUseCase` recusa o Inativo com mensagem própria (RN-005/RN-046).
- [x] Testes `[Trait("RuleId","RN-046")]`.
- [x] `dotnet build`/`dotnet test`/`check-harness.py`.

## Critérios de aceite

- Inativar Usuário Ativo → Inativo; reativar Inativo → Ativo; inativar/reativar fora dessas transições é recusado (conflito).
- Inativação que deixaria a plataforma sem Administrador do Sistema é recusada.
- Usuário Inativo não obtém acesso no login (recusa de negócio, credencial já validada).
- Sem schema novo (Status é coluna string existente).

## Evidências

- Backend: `rtk dotnet test` **328/328** (novos `[Trait("RuleId","RN-046")]`: inativar ativo→Inativo; reativar inativo→Ativo; recusa último Administrador do Sistema; permite quando há outro admin; recusa inativar já-inativo; NotFound). `check-harness.py` → `harness ok`.
- Sem migration (Status é coluna string existente; `EUserStatus.Inactive` persiste como string por convenção ADR-031). Glossário: status Inativo + transições Ativo↔Inativo (proposto na fatia 0).
- Login: `AuthenticateUserUseCase` recusa o Inativo com mensagem distinta da de Pendente (RN-005/RN-046), após validar credencial.
- Pendências: RN-046 por escopo/multi-Corretora ([OPEN-12]); RN-043/044/045 (catálogo de Permissões, Perfis customizados, enforcement); AB#/PBI; PR.
