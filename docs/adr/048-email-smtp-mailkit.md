---
id: ADR-048
title: E-mail via SMTP (MailKit) atrás de IMailService
status: accepted
tags: [integracoes]
applies-to: ["src/*.MailServices/**"]
supersedes: []
evidence: []
---

# ADR-048: E-mail via SMTP (MailKit) atrás de IMailService

## Status

Aceito

## Decisão (normativa)

- Envio de e-mail DEVE usar MailKit/MimeKit sobre SMTP, encapsulado atrás de `IMailService` (contrato no Core).
- O serviço DEVE receber o corpo HTML pronto e anexos como bytes em memória; montagem de template NUNCA vive no projeto de mail.
- A configuração SMTP DEVE ser uma única classe Options (ADR-053); classes de configuração duplicadas NUNCA existem.
- Falha de envio DEVE ser logada com contexto; o chamador decide se a falha é crítica para a operação.
- Provedores de e-mail transacional por API própria NUNCA são adicionados sem substituir esta ADR.

## Contexto

SMTP genérico mantém o provedor de e-mail trocável por configuração (dev usa sandbox, produção usa o relay contratado). Separar montagem de conteúdo (responsabilidade de quem conhece o template) de transporte (responsabilidade do MailService) evita que o projeto de mail acumule regra de negócio.

## Consequências

Templates vivem junto dos UseCases que os usam. O projeto de mail permanece pequeno e estável. Recursos específicos de provedores por API (tracking, supressão) ficam indisponíveis enquanto esta decisão valer.
