# Confiabilidade — a definir

Status: **fora do escopo do harness por enquanto.**

As garantias operacionais do produto (idempotência de callback, auditoria de transição de status, tratamento de erro de integração, estratégia de migração, ambientes e observabilidade) são comportamento de produto ou decisão de arquitetura/infra: nascem como RN (quando a jornada for construída) ou junto com a arquitetura, seguindo o padrão de documentação e de ADR deste repositório.

O princípio de harness que rege isso já vale hoje: garantia crítica é *testada* no gate de PR, não presumida ([régua de qualidade](QUALITY_SCORE.md), [constituição](constitution.md)).
