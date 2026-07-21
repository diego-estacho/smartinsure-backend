# Exec-plan 0007 — Consulta de Crédito (RN-029..031)

Status: ativo — implementação cross-repo da consulta de limites de crédito do tomador via Motor de Cálculo (PlugV2 `GetPolicyHolderLimitsAndRates`).

Contexto obrigatório (ler antes de executar): AGENTS.md, ARCHITECTURE.md, docs/BACKEND.md, docs/SECURITY.md, ADRs do Motor de Cálculo, RNs da jornada Consulta de Crédito (`docs/product-specs/regras-de-negocio/consulta-de-credito.md`), glossário (termos Limite de Crédito/`CreditLimit` e Consulta de Crédito/`CreditInquiry`, ratificados em 2026-07-20), open-decisions (OPEN-08 — limite utilizado, registro manual e análise de assessoria FORA desta entrega).

## Objetivo

Fatia vertical (sem AB# — pendência conhecida): usuário seleciona Corretora e informa CNPJ de tomador; plataforma dispara uma consulta por Habilitação de Seguradora Ativa da Corretora via motor da Habilitação (RN-023), apresenta resultados agrupados por Seguradora (limites Tradicional/Judicial/Financeiro, taxas, validade) com resumo consolidado, tolera falha isolada por Seguradora (RN-030) e grava histórico imutável (RN-031). Repos: smartinsure-dbmigration (tabelas), smartinsure-backend (contrato + domínio), smartinsure-frontend (tela Consulta de Crédito).

## Tarefas

- [x] RN-029..031 catalogadas e aprovadas; glossário ratificado (2026-07-20)
- [x] Migration Flyway: tabelas `CreditInquiries` e `CreditInquiryResults` (guards, índices, FK Restrict) — `V20260720194041__criar-tabelas-credit-inquiries.sql`
- [x] Core: entidades `CreditInquiry`/`CreditInquiryResult`, abstrações de repositório e da operação de consulta no motor
- [x] Integration: operação `GetPolicyHolderLimitsAndRates` no PlugV2 (HTTP, header `application-key-v2`, BaseUrl/Key da Habilitação, `Insurer.ReferenceExternalId` como identificador externo)
- [x] Infra.Data: mappings EF (precisão monetária explícita), repositório, DI
- [x] Application: UseCase de execução da Consulta de Crédito (fan-out por Habilitação Ativa, falha isolada, persistência do snapshot) + listagem/histórico
- [x] Api: endpoints Carter (`credit-inquiries`), contrato OpenAPI com WithName/WithSummary/Produces
- [x] Testes xUnit com `[Trait("RuleId", "RN-029|030|031")]`
- [x] Frontend: página `/consulta-credito` (kit Si), BFF Nitro, composable, types gerados (`npm run types:gen`), menu, testes Vitest `describe('RN-029 ...')` + E2E Playwright
- [x] Verificação: gates dos três repos + `python scripts/check-harness.py`
- [ ] PRs linkados (mesmo vínculo; AB# pendente)

## Critérios de aceite

- `dotnet build` e `dotnet test tests/SmartInsure.Tests` verdes (cobertura ≥ 80%)
- Front: `npm run lint`, `npm run typecheck`, `npm run test`, `npm run build` verdes
- `python scripts/check-harness.py` verde
- CNPJ inválido recusado antes de qualquer chamada ao motor; Corretora sem Habilitação Ativa recusada com mensagem (RN-029)
- Falha do motor em uma Seguradora não impede as demais; resumo considera só quem respondeu (RN-030)
- Cada consulta concluída gera registro imutável; reconsulta gera registro novo (RN-031)
- Limite utilizado NÃO exibido como zero (OPEN-08)
- Testes de RN carregam o ID (rastreabilidade derivada por script)

## Evidências

- Build backend: `dotnet build` — 0 avisos, 0 erros
- Testes backend: `dotnet test tests/SmartInsure.Tests` — 294/294 aprovados (41 novos da feature, `[Trait("RuleId","RN-029|030|031")]`)
- Cobertura da feature: 73,5% linhas nas classes novas (UseCases/Validator/entidades/PlugV2 ≥ 83–100%; repositório e mappings EF sem teste, consistente com a baseline do repo — nenhum repositório possui teste hoje)
- Harness: `python scripts/check-harness.py` → ok (backend e frontend)
- Contrato: `docs/generated/openapi.json` regenerado com `/api/v1/credit-inquiries` (POST/GET/GET{id}) — +452 linhas
- Frontend: `npm run lint`, `npm run typecheck`, `vitest run` 83/83, `npm run build` verdes; Playwright 24/24 (8 novos da jornada Consulta de Crédito)
- Review em dois eixos: 2 bugs (typo `traditionLimit`/`traditionRate` no DTO PlugV2) e 4 comparações frágeis de enum via `ToString()` corrigidos; achados de rota/UI pt-BR descartados (AGENTS.md do front determina pt-BR)
- Migration: validação local via `docker compose --profile migrations up -d` BLOQUEADA (Docker Desktop não inicia nesta máquina) — validar pela aplicação do CI no push pra `develop`
- Pendência conhecida: AB# não informado; vincular nos PRs quando existir
