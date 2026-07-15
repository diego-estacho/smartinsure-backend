# Jornada: Integração com Birô

## RN-003 — Consulta de dados cadastrais no Birô

**Descrição.** A plataforma obtém dados cadastrais de pessoa ou empresa consultando um Birô externo a cada solicitação. Nenhuma resposta anterior é reaproveitada: toda solicitação gera uma nova consulta ao Birô.

**Pré-condições.** Quem solicita informa o CPF/CNPJ, o tipo de pessoa no contexto da solicitação e o Birô desejado. O único Birô homologado nesta fase é a ReceitaWS.

**Critério de aceitação.** Dada uma solicitação com CPF/CNPJ, tipo de pessoa e Birô homologado, a plataforma consulta o Birô e devolve os dados cadastrais retornados. Duas solicitações idênticas e consecutivas geram duas consultas ao Birô.

**Casos limite.** Resposta do Birô indicando insucesso na fonte (status diferente de OK) é tratada como consulta sem dado, não como dado cadastral válido. Birô solicitado não homologado é recusado com indicação clara do motivo.

## RN-004 — Falha do Birô não bloqueia o fluxo solicitante

**Descrição.** A consulta ao Birô é enriquecimento de dados: indisponibilidade ou erro do Birô não impede a conclusão do fluxo que a solicitou.

**Pré-condições.** Um fluxo da plataforma solicitou consulta ao Birô.

**Critério de aceitação.** Com o Birô indisponível ou retornando erro, o fluxo solicitante conclui normalmente sem o dado complementar, e a falha fica registrada de forma consultável pela operação.

**Casos limite.** Falha por tempo de resposta excedido recebe o mesmo tratamento de indisponibilidade. Se um fluxo futuro exigir o dado do Birô como obrigatório, essa exigência nasce como RN própria daquele fluxo — não altera esta.
