# Exec-plan 0005 — Motor de Cálculo e Habilitação de Seguradora (RN-022..RN-024)

Status: em execução — backend e migration implementados; PRs pendentes
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, RNs em `docs/product-specs/regras-de-negocio/motor-de-calculo.md`, RN-004 (`integracao-biro.md`, padrão de falha de integração), glossário (termos Motor de Cálculo e Habilitação de Seguradora — propostos 2026-07-19), OPEN-07 (cotar Ofertas fica fora desta entrega).

## Objetivo

Infraestrutura do Motor de Cálculo: cadastro da Habilitação de Seguradora (vínculo Corretora×Seguradora com motor, parâmetros de conexão e status — RN-022) e resolução dinâmica do motor pela Habilitação (RN-023), com o PlugV2 como único motor registrado. O disparo do cotar Ofertas NÃO entra (OPEN-07); RN-024 será implementada junto com o cotar, quando existir operação de cotação para exercitar a falha.

## Tarefas

- [x] RN-022..RN-024 catalogadas em `motor-de-calculo.md`; glossário com termos novos (propostos, aguardando ratificação da PO); OPEN-07 registrada.
- [x] Migration no `smartinsure-dbmigration`: `V20260720052253__criar-tabela-brokerage-insurer-enablements.sql` (FK `BrokerageId → Persons`, `InsurerId → Insurers`; unique no par; Status string; parâmetros de conexão).
- [x] Core: `BrokerageInsurerEnablement` (entidade rica), `EBrokerageInsurerEnablementStatus`, `ECalculationEngine` (PlugV2), `IBrokerageInsurerEnablementRepository`, `ICalculationEngine` + `ICalculationEngineResolver`.
- [x] Integration: `CalculationEngines/` (`PlugV2CalculationEngine`, `CalculationEngineResolver` com motores por keyed DI); client Refit do PlugV2 entra com a primeira operação (OPEN-07).
- [x] Parametrização por vínculo: `ConnectionParameters` do PlugV2 = `{"baseUrl","key"}` validado na gravação (`PlugV2ConnectionParameters.Parse`); CNPJ da Corretora vem do vínculo; `Insurer.ReferenceExternalId` (opcional, migration `V20260720055100`) guarda o id da Seguradora no sistema de origem. Sem configuração global de motor. Pendência: avaliar cifragem da `key` (credencial em coluna).
- [x] Application: use cases Create/List/Get/Update/ChangeStatus da Habilitação.
- [x] Infra.Data: mapping 1:1 com a migration, repository, DI.
- [x] Api: `BrokerageInsurerEnablementsEndpoint` (autenticado, sem restrição de Perfil nesta fase — OPEN-07/OPEN-03).
- [x] Testes com `[Trait("RuleId", "RN-022")]` e `RN-023`; RN-024 fica para a demanda do cotar (OPEN-07).
- [x] Validar migration localmente (`docker compose --profile migrations up -d`).
- [x] Contrato `openapi.json` publicado.
- [ ] PRs: dbmigration (→ develop) antes/junto do backend (→ main), mesmo vínculo de atividade (AB# pendente — slug provisório `motor-de-calculo`).

## Critérios de aceite

- `dotnet build` e `dotnet test` verdes; `python scripts/check-harness.py` verde.
- Par Corretora×Seguradora único: segunda Habilitação recusada (RN-022).
- Resolver: Habilitação Ativa com PlugV2 → instância do motor com os parâmetros do vínculo; sem Habilitação/Inativa/motor indisponível → recusa com mensagem de não habilitada; Seguradora Inativa → recusa (RN-023/RN-010).
- Habilitação nunca excluída; alternância Ativa↔Inativa idempotente-negada (conflito ao repetir estado).

## Evidências

- Backend: `dotnet build` sem erros; `dotnet test` — 220/220 aprovados (19 novos: RN-022 create/update/status, RN-023 resolver); `check-harness.py` → `harness ok`.
- Migration: Flyway local aplicou `criar-tabela-brokerage-insurer-enablements` com sucesso (agora em v20260720052253).
- Contrato: `docs/generated/openapi.json` com `/api/v1/brokerage-insurer-enablements` (POST/GET), `/{id}` (PUT/GET) e `/{id}/status` (PATCH).
- Pendências: ratificação da PO (termos e RNs), AB#/PBI, PRs.
