---
id: ADR-103
title: Cobertura Adicional na Cotação é enviada pelo nome da Importada, não pelo identificador de origem
status: accepted
tags: [integracao, plugv2, cotacao, cobertura-adicional]
applies-to: ["src/*.Application.UseCase/**", "src/*.Core/**", "src/*.Integration/**"]
supersedes: []
evidence: ["probe /Cotation no gateway QA, 2026-08-04, corretora Finn", "propostas 202600000274306/274307 em db-garantia-qa-us — decomposição do prêmio"]
---

# ADR-103: Cobertura Adicional na Cotação enviada pelo nome da Importada

## Status

Aceito (2026-08-04), AB#0007. Sustenta RN-105 e RN-106. Decisão tomada **sobre evidência
experimental**, não sobre leitura do modelo de dados — que apontava na direção oposta.

## Decisão (normativa)

- O campo `AdditionalCoverages` do `POST /Cotation` recebe o **nome** da Cobertura Adicional
  Importada com que a Seguradora expõe a cobertura na Modalidade cotada. **Nunca** o
  `SourceUniqueId` (identificador de origem).
- Uma Cobertura Adicional canônica escolhida contribui com **exatamente um** nome. Se a resolução
  produzir zero nomes (a Seguradora não oferece) ou mais de um nome distinto (grafias divergentes
  entre ramos — OPEN-22), a cobertura **não é enviada** e consta como não contemplada (RN-106).
- **Jamais enviar superset de nomes.** Cobertura não suportada faz o gateway recusar a solicitação
  inteira, derrubando a Cotação — a plataforma erra para menos, nunca para mais.
- A resolução canônica → nome é **regra de negócio, na Application** (`IQuotationAdditionalCoverageResolver`),
  não na camada anticorrupção do motor. A ACL traduz modelo de fornecedor (ADR-045, ADR-028) e não
  pode depender de repositório de catálogo — a regra de dependência de camadas é gate de teste.
- O `SourceUniqueId` **continua sendo importado** (RN-041): serve de rastro na Cotação
  (`ImportedAdditionalCoverageId`) e destrava a troca de estratégia se o gateway passar a aceitar
  identificador.

## Contexto

`AdditionalCoverages` existe no contrato do `POST /Cotation` desde o fan-out (exec-plan 0013), mas
sempre foi enviado **vazio** — o código carregava um `TODO(probe T14)` porque o formato aceito nunca
tinha sido confirmado. Efeito prático: toda Cotação era precificada apenas com a garantia principal,
independentemente do que o corretor marcasse na etapa 3, e nada avisava o usuário.

Dois sinais apontavam para o identificador:

1. O SmartInsure importa `SourceUniqueId` de **todas** as Coberturas Adicionais Importadas (249 de
   249 linhas preenchidas no catálogo de QA), enquanto o `GetAdditionalCoverages` devolve `Name` e
   `UniqueId`.
2. Os **nomes de origem são caóticos** — o mesmo conceito aparece como `Multa` (6 Seguradoras),
   `Multas` (3), `MULTAS E PENALIDADES`, `Adicional de Multas e Penalidades`; e
   `Trabalhista e Previdenciário` (3), `Ações Trabalhistas e Previdenciárias` (2),
   `Trabalhistas, Sociais e Previdenciárias`, `Trabalhista e Previdenciária`. Nome parecia chave
   frágil.

A plataforma legada, por outro lado, **não tem** coluna de identificador em
`ImportedAdditionalCoverage` (só `Name` + `BranchCode`), logo só poderia enviar nome. O probe
resolveu a contradição.

## Evidência (probe ao vivo, gateway QA, corretora Finn, 2026-08-04)

Mesma Cotação, variando apenas `AdditionalCoverages`:

```
["b4e65794-032b-4210-bfc4-6eef35210833"]      (SourceUniqueId de "Multas", ramo Public da AXA)
→ HTTP 400
  "Atenção! Existem coberturas informadas na criação da cotação que não são suportadas:
   b4e65794-032b-4210-bfc4-6eef35210833"

["Multas"]                                     (nome da Importada)
→ HTTP 200, ResponseStatus.Status = 5 (Requer análise de subscrição)
```

O gateway **nomeia o valor recusado** na mensagem de erro, o que torna a conclusão inequívoca: o
identificador de origem não é valor aceito no campo; o nome é.

Dois achados operacionais do mesmo probe:

- **Cobertura não suportada derruba a solicitação inteira** (HTTP 400), não é ignorada. É a razão
  normativa de nunca enviar superset.
- O **dedup do gateway** (`"Já existe uma cotação para esta cotação"`, `CommonValidationException`)
  ignora variação de valor segurado **e** de vigência. Reforça RN-057 (a chamada não é idempotente e
  não se re-tenta) e limita probes futuros a uma chamada avaliada por tupla.

### Aplicação confirmada no lado da Seguradora (2026-08-04)

O probe provou que o nome é **aceito**; a confirmação de que a cobertura é **aplicada** veio depois, do
banco da Seguradora (`db-garantia-qa-us`), sobre duas propostas criadas no mesmo fan-out, com o mesmo
risco e no mesmo instante — uma com a cobertura enviada, outra sem:

| Proposta | Enviado | `ProposalCoverage` | Prêmio principal | Coberturas | Prêmio total |
|---|---|---|---|---|---|
| 202600000274307 (Sombrero) | `["Multas"]` | 1 linha: `Multas`, `IsAdditionalCoverage = true` | 150,00 | **+75,00** | **225,00** |
| 202600000274306 (SANCOR) | `[]` | nenhuma | 250,00 | — | 250,00 |

Três fatos que isso estabelece:

1. **O nome é resolvido, não ignorado.** A proposta ganhou linha em `ProposalCoverage` apontando para a
   `Coverage` de Cobertura Adicional correspondente.
2. **O prêmio reflete a cobertura.** `InsurancePremium` (225,00) = `InsurancePremiumPrincipalModality`
   (150,00) + soma das coberturas adicionais (75,00). Um terço do prêmio vem da cobertura adicional.
3. **A Seguradora resolve o ramo, e resolve certo.** O `Coverage.UniqueId` gravado na proposta é
   `1B968A60-5FBE-4704-B0A1-82136ED7A6E6` — exatamente o identificador de origem da "Multas" da
   Sombrero no ramo **Public**, o mesmo GUID que o gateway **recusou** quando enviado diretamente no
   probe. Enviamos o nome; ela escolheu a linha do ramo correto (o Segurado é um município).
   Isto confirma experimentalmente a consequência central desta decisão: enviando nome, a resolução de
   ramo é responsabilidade da Seguradora — não é inferência, é medição.

Também confirma que `NotOffered` é fiel: a proposta da SANCOR existe, tem zero coberturas e prêmio
correspondente apenas à garantia principal — cotada sem a cobertura, como a regra determina (RN-106).

## Consequências

- O ramo (Público/Privado) **deixa de ser problema no envio**: cada Modalidade canônica mapeia para
  duas Modalidades Importadas por Seguradora, uma por ramo, com identificadores distintos mas
  frequentemente o **mesmo nome**. Enviando nome, quem resolve o ramo é o gateway — **medido no
  ambiente de QA** (ver a seção de aplicação confirmada: a proposta gravou o `UniqueId` do ramo
  correto). Coerente com o fan-out, que já envia apenas `ModalityGlobalId`. Resta só o caso de nomes
  divergentes (OPEN-22).
- A Cotação registra o nome enviado (`SentName`), o que dá rastro auditável do que a Seguradora
  recebeu — útil porque o nome é dado da origem e pode mudar entre importações.
- A qualidade da **curadoria** (RN-043) passa a afetar diretamente a precificação: canônica sem
  vínculo não é ofertada nem enviada. Catálogo de QA com lixo de teste (`teste`, `asdf`, `sdfsdd`)
  polui a curadoria e merece limpeza à parte.

## Alternativas rejeitadas

- **Enviar `SourceUniqueId`.** Recusado pelo gateway com erro explícito (evidência acima).
- **Enviar o conjunto de nomes vinculados à canônica.** Um nome não suportado no ramo resolvido faz
  o gateway recusar a solicitação inteira — troca uma cobertura faltante por uma Cotação perdida.
- **Resolver na ACL do PlugV2.** Precisaria de repositório de catálogo dentro da camada de
  Integration, violando a regra de dependência de camadas (ADR-045, ADR-028) — que é gate de
  NetArchTest.
- **Congelar a resolução na criação do Grupo (snapshot).** Cria uma segunda fonte de verdade que
  envelhece enquanto o Grupo fica em Rascunho, contra o "derivado dos vínculos ativos, nunca
  digitado" de RN-046.
