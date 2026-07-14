---
id: ADR-050
title: Processamento assíncrono com Channel, reconciliador e idempotência
status: accepted
tags: [assincrono, application]
applies-to: ["src/*.Infra.BackgroundServices/**", "src/*.Application.UseCase/**"]
supersedes: []
evidence: []
---

# ADR-050: Processamento assíncrono com Channel, reconciliador e idempotência

## Status

Aceito

## Decisão (normativa)

- Operação longa DEVE retornar 202 Accepted e enfileirar um work item num `Channel<T>` bounded, cujo contrato (`I{Operação}Channel`) vive no Core.
- O consumo DEVE ser um `BackgroundService` que cria scope de DI próprio por work item e aplica controle de concorrência por recurso externo (`SemaphoreSlim` por destino).
- Todo processamento em memória DEVE ter reconciliador periódico: varre o estado persistido e reenfileira itens perdidos (restart/deploy no meio do processamento).
- Operações assíncronas DEVEM ser idempotentes (reprocessar o mesmo item não duplica efeito); retry NUNCA é configurado para operação não idempotente.
- `CancellationToken` DEVE ser propagado em toda a cadeia assíncrona; método async sem parâmetro de cancelamento é desvio.
- O estado da operação DEVE ser persistido (banco) antes do enfileiramento — a fila em memória nunca é o único registro do trabalho pendente.

## Contexto

Channel in-process dá processamento assíncrono sem infraestrutura de mensageria, mas perde o conteúdo da fila em restart. O reconciliador fecha essa lacuna lendo o estado persistido como fonte da verdade — a fila vira otimização de latência, não registro. A idempotência é o que torna o reprocessamento (por reconciliador ou retry) seguro.

## Consequências

Não há broker para operar; a garantia de entrega vem do par persistência + reconciliador, com latência de recuperação igual ao período de reconciliação. A escala horizontal do consumo exige revisitar esta ADR (fila é por instância).
