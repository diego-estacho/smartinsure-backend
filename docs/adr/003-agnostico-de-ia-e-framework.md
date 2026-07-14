# ADR-003: Agnóstico de ferramenta de IA e de framework de desenvolvimento

## Status
Proposto (ratificação na Fase A)

## Contexto
A empresa já trocou de ferramenta de IA e cada dev tem preferências próprias de framework de desenvolvimento (speckit, grill-me, ou nenhum). O harness não pode ficar preso a uma ferramenta.

## Decisão
O `AGENTS.md` é o ponto de entrada canônico; `CLAUDE.md` e equivalentes são ponteiros de uma linha. Todo enforcement crítico vive em repo + CI.

**O harness valida o resultado, não a ferramenta:** cada dev usa o framework que preferir, desde que o resultado respeite o padrão do projeto — documento no formato dos templates, RN aprovada pela PO, decisão registrada como ADR e gates verdes. Segurança de produção por credencial read-only no servidor, não por hook de cliente.

## Consequências
- O conhecimento e a verificação vivem no repositório; a ferramenta é peça trocável (plug-and-play).
- Nenhum framework de desenvolvimento é prescrito nem proibido.
- (Pendente) a fronteira das pastas próprias de cada framework será definida em ADR próprio, para as pastas de cada kit não poluírem o repo.
