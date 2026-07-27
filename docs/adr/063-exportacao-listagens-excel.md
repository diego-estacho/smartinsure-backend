---
id: ADR-063
title: Exportação de listagens para Excel via serviço reutilizável
status: accepted
tags: [application, api, integracoes]
applies-to: ["src/*.Infra.CrossCutting/Export/**", "src/*.Api/Endpoints/**"]
supersedes: []
evidence: []
---

# ADR-063: Exportação de listagens para Excel via serviço reutilizável

## Status

Aceito

## Decisão (normativa)

- A exportação de listagens para planilha DEVE usar um serviço reutilizável `IExcelExporter`
  (contrato no Core), com implementação única via **ClosedXML** (licença MIT) em
  `Infra.CrossCutting`. Cada tela declara apenas suas colunas (`ExcelColumn<T>`: cabeçalho +
  seletor de valor); a montagem da planilha NUNCA é duplicada por tela.
- A geração é **síncrona** nesta fase, com **teto de segurança de 10.000 linhas**. A exportação
  reaproveita a MESMA consulta e os MESMOS filtros da listagem correspondente, sem paginação até
  o teto — nunca duplica a lógica de filtro do repositório.
- O endpoint de exportação DEVE devolver o arquivo com `Results.File` (content-type
  `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`) e nome de arquivo próprio;
  herda a autorização por default do grupo `api/v1` (autenticado, ADR-010), sem policy adicional
  nesta fase.
- Bibliotecas de planilha com licença restritiva a uso comercial (ex.: EPPlus ≥ 5) NUNCA são
  adicionadas enquanto esta decisão valer.
- Volumes acima do teto ou geração demorada DEVEM migrar para o padrão assíncrono (ADR-050/051)
  quando o negócio exigir — decisão a registrar então, substituindo o modo síncrono aqui.

## Contexto

A exportação nasce na listagem de Corretoras (RN-018) e se repetirá em outras listagens
(Cotações, Tomadores, etc.). Um serviço genérico dirigido por definição de colunas evita
reimplementar geração de `.xlsx` a cada tela. O modo síncrono atende o volume atual (base típica
bem abaixo do teto) sem o custo de infraestrutura do fluxo assíncrono. ClosedXML é MIT — evita o
risco de licença comercial do EPPlus moderno.

## Consequências

Nova dependência `ClosedXML` no projeto de cross-cutting. Exportações ficam limitadas a 10.000
linhas por chamada até que o fluxo assíncrono seja adotado. Cada nova tela exportável só precisa
declarar suas colunas e um use case/endpoint fino que reaproveita sua consulta de listagem.
