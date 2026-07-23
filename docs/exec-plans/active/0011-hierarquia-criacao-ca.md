# Exec-plan 0011 — Hierarquia de criação: Administrador convida Corretor Administrador (RN-036, fatia 3a)

Status: em andamento — fatia 3a da jornada Perfis e Permissões (slug `perfis-permissoes`, AB# pendente). Sobre as fatias 0/1/2.
Contexto obrigatório: `AGENTS.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, RN-036/RN-037/RN-038 (`regras-de-negocio/perfis-e-permissoes.md`), RN-012 (`usuarios.md`), glossário (`BrokerageAdministrator`/`PolicyHolderAdministrator`), ADR-013/ADR-014 (autorização por policy/claims), [OPEN-11](../../product-specs/open-decisions.md).

## Objetivo

Primeiro degrau da hierarquia: o Administrador do Sistema convida um Corretor Administrador informando as Corretoras (RN-036). Semeia os Perfis fixos globais Corretor Administrador e Tomador Administrador (RN-032/RN-043). Reusa o Convite de primeiro acesso (RN-035, fatia 2).

## Escopo e não-escopo (fatia 3a)

- **No escopo:** seed dos Perfis fixos CA/TA (migration); `InviteBrokerageAdministratorUseCase` (só Administrador do Sistema — Policy `SystemAdministrator`; cria Usuário Pendente + Convite + um vínculo Corretor Administrador por Corretora informada; recusa sem corretora / corretora inexistente ou inativa / e-mail duplicado); endpoint `POST /users/brokerage-administrators`.
- **Fora do escopo (diferido, com motivo):**
  - **RN-037** (ampliar Corretoras de um Usuário): o Perfil atribuído na ampliação "segue as regras de criação (RN-038 a RN-041)" — subespecificado pela RN-037 isolada; acopla às regras de criação de Perfil por escopo (fatia 4, parte sob OPEN-09). Implementar sem isso seria inventar qual Perfil atribuir. Fica para depois da fatia 4.
  - **RN-038** (Corretor Administrador cria Tomador Administrador): exige a **Corretora ativa** (fatia 1b), travada pela [OPEN-11](../../product-specs/open-decisions.md) (ADR-060 proposto + padrão de primeiro acesso PO).

## Tarefas

- [x] Migration `V20260723174640__seed-perfis-fixos-ca-ta.sql` (Perfis fixos CA/TA, GUIDs estáveis, guards).
- [x] `ProfileNames.BrokerageAdministrator`/`PolicyHolderAdministrator`; `IProfileRepository.GetBrokerageAdministratorAsync` (chave natural) + impl.
- [x] `InviteBrokerageAdministratorUseCase` (+ Request/Response/Validator/Interface) reusando o fluxo de Convite (commit único, e-mail pós-commit fora da compensação de identidade).
- [x] Endpoint `POST /users/brokerage-administrators` com `RequireAuthorization(Policies.SystemAdministrator)`.
- [x] Testes `[Trait("RuleId","RN-036")]`.
- [x] `dotnet build`/`dotnet test`/`check-harness.py`.

## Critérios de aceite

- Só o Administrador do Sistema convida CA (policy fail-closed); convite com nenhuma corretora, corretora inexistente ou inativa é recusado; e-mail duplicado recusado sem criar identidade.
- Usuário nasce Pendente com Convite (RN-035) e um vínculo Corretor Administrador por Corretora informada.
- Perfis fixos CA/TA resolvidos por chave natural (IsFixed+Scope+Name), nunca por GUID.

## Evidências

- Backend: `rtk dotnet test` **322/322** (novos `[Trait("RuleId","RN-036")]`: convida CA com um vínculo por corretora; recusa corretora inativa/inexistente; recusa e-mail duplicado sem criar identidade). `check-harness.py` → `harness ok`.
- Reuso do padrão fatia 2 (commit único Usuário+Convite+vínculos; e-mail pós-commit fora da compensação) e do precedente RN-027 pra validar Corretora Ativa (`GetTrackedBrokerageByIdAsync` + `GetRole(Broker).Status`).
- DI por convenção (ADR-021, assembly scan) — sem registro manual. Perfil fixo resolvido por chave natural.
- Migration `V20260723174640__seed-perfis-fixos-ca-ta.sql` **NÃO aplicada** (segue o gate; incluída no `aplicar-vps.ps1` após as demais).
- Nota de execução: um subagente reportou esta fatia como concluída sem ter criado arquivo algum (relatório alucinado); a fatia foi implementada e verificada manualmente.
- Pendências: aplicar migration; RN-037 (após fatia 4) e RN-038 (após fatia 1b/OPEN-11); AB#/PBI; PR.
