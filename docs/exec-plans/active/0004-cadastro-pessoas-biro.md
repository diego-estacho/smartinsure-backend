# Exec-plan 0004 — Cadastro de Pessoas via Birô (RN-013..RN-016)

Status: em execução — backend e migrations implementados; PRs pendentes
Contexto obrigatório (ler antes de executar): `AGENTS.md`, `ARCHITECTURE.md`, `docs/BACKEND.md`, `docs/SECURITY.md`, RNs em `docs/product-specs/regras-de-negocio/pessoas.md`, RN-003/RN-004 (`integracao-biro.md`), glossário (termos Pessoa Jurídica, Segurado, Tomador, Natureza Jurídica, Matriz/Filial — propostos 2026-07-16), OPEN-04 (parcialmente resolvida por esta entrega).

## Objetivo

Fatia vertical da jornada Cadastro de Pessoas: busca de Pessoa Jurídica por nome ou documento (RN-013), importação automática do Birô quando o CNPJ não está na base (RN-014), classificação público/privado pela Natureza Jurídica (RN-015) e resolução de matriz para tomador (RN-016). Escopo restrito a segurado/tomador — corretora/seguradora via Birô exige revisão da RN-007 (fora desta entrega).

## Tarefas

- [x] RN-013..RN-016 catalogadas em `pessoas.md`; glossário com os termos novos (propostos, aguardando ratificação da PO).
- [x] Migrations no `smartinsure-dbmigration` (branch `rn-013-cadastro-pessoas`): `V20260716153951__criar-tabela-legal-natures.sql` (tabela + seed CONCLA, 94 códigos) e `V20260716154100__criar-tabelas-persons.sql` (Persons + PersonAddresses).
- [x] Core: `Person`, `PersonAddress`, `LegalNature` (entidades ricas), `EPersonRole`, `IPersonRepository`, `ILegalNatureRepository`.
- [x] CrossCutting: `CnpjValidator.IsHeadquarters`/`HeadquartersOf` (resolução da matriz com DVs recalculados).
- [x] Application: `SearchPersonsUseCase` (busca → importação Birô → matriz para tomador), validator.
- [x] Infra.Data: mappings (alinhados 1:1 com as migrations), `PersonRepository`, `LegalNatureRepository`, DbSets e DI.
- [x] Api: `PersonsEndpoint` (GET /persons?term&role — autenticado).
- [x] Testes com rastreabilidade `[Trait("RuleId", "RN-013".."RN-016")]`.
- [x] RN-017 (vínculo de papel da Pessoa): entidade `PersonRole`, `Person.AssignRole` idempotente, migration `V20260716192848__criar-tabela-person-roles.sql`, vínculo automático na devolução por documento/importação (busca por nome não vincula), papéis expostos na resposta; testes com `RuleId RN-017`.
- [x] Validar migrations localmente (`docker compose --profile migrations up -d`).
- [ ] OPEN-04: registrar decisão parcial (gatilho busca-por-documento + preenchimento de cadastro) após ratificação da PO.
- [ ] Contrato `openapi.json` publicado; front consome depois (mesmo `AB#NNNNN`, ainda sem PBI).
- [ ] PRs: dbmigration (→ develop) antes/junto do backend (→ main).

## Critérios de aceite

- `dotnet build` e `dotnet test` verdes; testes de arquitetura passando.
- `python scripts/check-harness.py` verde.
- Teste de RN carrega o ID (gate de rastreabilidade).
- Busca com CNPJ já cadastrado não gera chamada ao Birô (RN-013); falha do Birô não bloqueia (RN-004/RN-014).

## Evidências

- Backend: `dotnet test` — 173/173 aprovados (inclui 18 testes novos RN-013..RN-016); `dotnet build` sem erros; `check-harness.py` → `harness ok`.
- Migrations: Flyway local aplicou `criar-tabela-legal-natures` e `criar-tabelas-persons` com sucesso (v20260716154100); seed conferido no SQL Server local — 94 naturezas, 35 do setor público.
- Pendências registradas nas tarefas: ratificação da PO (termos e RNs), contrato openapi/front, PBI/PRs.
