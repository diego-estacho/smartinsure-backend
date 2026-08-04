---
id: ADR-102
title: Persistir payloads da integração PlugV2 no MongoDB (QuotationIntegrationLog) com TTL
status: proposed
tags: [integracao, observabilidade, mongo, plugv2]
applies-to: ["src/*.Integration/**", "src/*.Infra.Data/**", "src/*.Api/**"]
supersedes: []
evidence: []
---

# ADR-102: Log de integração PlugV2 no MongoDB (QuotationIntegrationLog) com TTL

## Status

Proposto (2026-08-01). Task estrutural (observabilidade), sem RN — não é comportamento de negócio, e sim rastreabilidade da integração com o provedor de cálculo. Habilita validar/depurar a cotação real (payloads request/response, veredito, falhas do gateway como o dedup "já existe uma cotação").

## Decisão (normativa)

- Cada solicitação de Cotação ao PlugV2 (`POST /Cotation`) grava **um documento** na collection **`QuotationIntegrationLog`** do MongoDB (a base de logs/payloads já prevista na ARCHITECTURE — primeiro consumidor real do `IMongoRepository<>`).
- **Escopo:** apenas `/Cotation` nesta fase (a chamada que cria a proposta e carrega o veredito/erros do gateway). Demais chamadas PlugV2 (limites, minuta) ficam fora.
- **Segredo nunca é logado:** a PlugKey trafega no header `application-key-v2`, não no corpo. Loga-se **somente o corpo** (request e response); headers nunca (SECURITY.md).
- **Truncamento:** cada payload (request e response) é truncado em **256 KB**, com flag `Truncated` por lado.
- **Retenção via TTL:** o documento carrega `ExpiresAtUtc = CreatedAtUtc + Mongo:IntegrationLogRetentionDays` (default **30**, configurável em appsettings). Um índice TTL em `ExpiresAtUtc` (`expireAfterSeconds: 0`) expira o documento por data. O índice é **garantido pelo app no startup**.
- **Best-effort:** falha ao gravar o log **nunca** derruba a cotação — o recorder captura a exceção e apenas emite um warning.
- **CorrelationId:** capturado do `Activity` corrente (formato W3C) no momento da gravação, para ligar com o App Insights/OpenTelemetry.
- **Infra:** o `mongo` passa a subir pelo `docker-compose` do backend (dev/QA); a connection string já existia no dev-config.

## Contexto

Até aqui a integração PlugV2 não deixava rastro persistido: depurar "por que o gateway recusou" dependia de logs voláteis. O caso do dedup de 60s ("já existe uma cotação para esta cotação") mostrou a necessidade de ver o par request/response real. O Mongo já estava previsto para logs/payloads/auditoria (ARCHITECTURE), mas sem consumidor; esta é a primeira collection.

Alternativas consideradas: logar em App Insights apenas (payloads grandes/custo e retenção rígida); logar em SQL (payloads grandes não cabem bem no modelo relacional); não persistir (perde a evidência). Mongo com TTL equilibra volume, retenção automática e consulta ad-hoc.

## Consequências

- Uma dependência de Mongo no caminho da cotação — mitigada por best-effort (a cotação não depende do log).
- O `docker-compose` ganha o serviço `mongo` (volume por host, compartilhado entre worktrees — mesma ressalva do mssql/azurite).
- O documento `QuotationIntegrationLog` mora no Core (tipo consumido por `IMongoRepository<>`), respeitando o gate de camadas (NetArchTest).
- **Em aberto (não decidido aqui):** estender o log às demais chamadas PlugV2 e a exposição de uma tela/консulta desses logs — ficam para demanda própria se necessário.
