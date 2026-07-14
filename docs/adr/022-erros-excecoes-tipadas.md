---
id: ADR-022
title: Erro de negócio por exceções tipadas
status: accepted
tags: [application, api]
applies-to: ["src/*.Application.UseCase/**", "src/*.Core/Exceptions/**", "src/*.Api/Handlers/**"]
supersedes: []
evidence: []
---

# ADR-022: Erro de negócio por exceções tipadas

## Status

Aceito

## Decisão (normativa)

- O UseCase DEVE sinalizar erro de negócio exclusivamente lançando exceções tipadas do domínio: `NotFoundException` (recurso inexistente), `ConflictException` (conflito de estado), `BusinessRuleException` (regra impede a operação).
- As exceções tipadas DEVEM viver no Core (`Core/Exceptions/`) e carregar mensagem em pt-BR pronta para o consumidor.
- O resolver central da API DEVE mapear cada tipo 1:1 para o ProblemDetails da ADR-012 (404, 409, 422).
- UseCase NUNCA retorna null/flag para sinalizar erro de negócio; NUNCA usa mecanismo paralelo (notification context, Result) para o mesmo fim.
- Exceções tipadas são para erro de negócio; falhas de infraestrutura NUNCA são convertidas em exceção de negócio.

## Contexto

O mapa HTTP semântico (ADR-012) exige que a Application comunique a categoria do erro. Exceções tipadas fazem isso com um único mecanismo, sem alterar a assinatura dos UseCases (como Result exigiria) e sem estado implícito por request (como notifications). Alternativas descartadas: Result pattern (muda todo call site) e notification context (não carrega categoria e permite esquecer a checagem).

## Consequências

Trade-off aceito: erros de negócio pós-validação não acumulam — a primeira falha aborta (múltiplos erros de formato continuam acumulando na validação FluentValidation, que roda antes). Controle de fluxo por exceção tem custo de performance irrelevante no volume esperado dessas rotas.
