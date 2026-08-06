# ADR-066: Emissão síncrona, num commit, sem reconciliação nesta fase

## Status
proposed (decisão do dono do produto em 2026-08-03; ratificação da PO pendente — OPEN-07)

## Contexto

A etapa de emissão (RN-500..RN-514) precisa executar, contra a Seguradora, uma sequência de operações
**mutantes e não repetíveis**: reenviar os termos da minuta (RN-502), comunicar o aceite do Termo
(RN-506) e solicitar a emissão da Apólice (RN-500), seguida do cancelamento das Cotações irmãs (RN-509).

Dois fatos do provedor moldam a decisão:

- O `CreatePolicy` do gateway **não é idempotente** — ele se protege com um lock de 30 minutos e
  responde "já existe um pedido de emissão para esta cotação" na segunda chamada. Mesma natureza do
  `/Cotation` que motivou a RN-057 (sem retry automático).
- O `CreatePolicy` devolve apenas a referência da apólice e o número da proposta. Número da apólice,
  arquivo e boletos exigem uma consulta posterior (`GetPolicy`), e **não existe callback/webhook** do
  provedor: qualquer confirmação é *pull*.

O fan-out de cotação já resolveu problema parecido com canal + consumidor + reconciliador (ADR-050) e
acompanhamento por polling (ADR-051). A pergunta era se a emissão deveria seguir o mesmo desenho.

Opções consideradas:

1. **Saga assíncrona com retomada** — pedido durável enfileirado, consumidor executa as etapas marcando
   progresso, reconciliador retoma a etapa que faltou.
2. **Saga com compensação** — igual à 1, mas cancelando a proposta no provedor quando a emissão falha.
3. **Sequência síncrona, num commit** — tudo dentro da requisição, commit no fim.

## Decisão

**Sequência síncrona, num único commit** (opção 3), por decisão do dono do produto.

- O portão de verificações (RN-500/501/502/505/507) roda **antes** de qualquer chamada mutante: recusa
  previsível não gasta chamada não repetível nem queima o aceite do Termo.
- Falha em qualquer etapa não registra Apólice e mantém o Grupo em **Cotado** (RN-508/RN-511).
- Proteção contra pedido duplicado é **estado nosso** (uma Apólice por Cotação, com índice único em
  `QuotationId`), não o lock do provedor: o lock é segunda linha, não a regra.
- O cancelamento das irmãs (RN-509) acontece **depois** do commit e seu insucesso não desfaz a emissão.
- **Não há job de reconciliação nesta fase**, e `GetPolicy` fica fora de escopo. Por isso a situação
  final do Grupo é **Emissão solicitada** (`EmissionRequested`), nunca "Emitida": a plataforma não
  afirma o que não confirmou.

## Consequências

- **Risco aceito explicitamente:** uma queda entre a resposta da Seguradora e o commit deixa a emissão
  solicitada no provedor sem registro na plataforma. A nova tentativa do corretor recebe do gateway a
  informação de que já existe pedido, e a mensagem é repassada — mas a plataforma não se auto-corrige.
  A reconciliação (consultar `GetPolicy`, registrar a Apólice órfã, confirmar número/arquivo/boletos) é
  demanda própria, registrada em OPEN-07.
- O corretor espera as chamadas ao provedor dentro da requisição; o teto de tempo é o do gateway.
- Quando a confirmação da emissão entrar, este ADR deve ser revisitado: o desenho natural é o da
  opção 1 (ADR-050/051 já dão o molde — canal, consumidor, reconciliador e polling).
- Rejeitada a opção 2 (compensar cancelando a proposta): um erro transitório de rede custaria a Cotação
  escolhida e obrigaria a recotar a oferta inteira, criando propostas novas no provedor.
