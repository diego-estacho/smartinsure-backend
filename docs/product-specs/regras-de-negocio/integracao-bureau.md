# Jornada: Integração com Bureau

## RN-003 — Consulta de dados cadastrais no Bureau

**Descrição.** A plataforma obtém dados cadastrais de pessoa ou empresa consultando um Bureau externo a cada solicitação. Nenhuma resposta anterior é reaproveitada: toda solicitação gera uma nova consulta ao Bureau.

**Pré-condições.** Quem solicita informa o CPF/CNPJ, o tipo de pessoa no contexto da solicitação e o Bureau desejado. O único Bureau homologado nesta fase é a ReceitaWS.

**Critério de aceitação.** Dada uma solicitação com CPF/CNPJ, tipo de pessoa e Bureau homologado, a plataforma consulta o Bureau e devolve os dados cadastrais retornados. Duas solicitações idênticas e consecutivas geram duas consultas ao Bureau.

**Casos limite.** Resposta do Bureau indicando insucesso na fonte (status diferente de OK) é tratada como consulta sem dado, não como dado cadastral válido. Bureau solicitado não homologado é recusado com indicação clara do motivo.

## RN-004 — Falha do Bureau não bloqueia o fluxo solicitante

**Descrição.** A consulta ao Bureau é enriquecimento de dados: indisponibilidade ou erro do Bureau não impede a conclusão do fluxo que a solicitou.

**Pré-condições.** Um fluxo da plataforma solicitou consulta ao Bureau.

**Critério de aceitação.** Com o Bureau indisponível ou retornando erro, o fluxo solicitante conclui normalmente sem o dado complementar, e a falha fica registrada de forma consultável pela operação.

**Casos limite.** Falha por tempo de resposta excedido recebe o mesmo tratamento de indisponibilidade. Se um fluxo futuro exigir o dado do Bureau como obrigatório, essa exigência nasce como RN própria daquele fluxo — não altera esta.
