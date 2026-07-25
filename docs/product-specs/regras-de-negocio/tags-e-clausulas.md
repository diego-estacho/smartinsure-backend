# Jornada: Tags e Cláusulas Particulares

Cada RN é uma seção com o ID no título e os quatro blocos abaixo. O ID é `RN-NNN` numa **sequência única do catálogo** (não reinicia por jornada), estável e nunca reaproveitado. Linguagem de negócio, termos do [glossário](../glossario.md), sem path de código no corpo.

> Origem: PRD "Tags (Objeto da Modalidade)" (2026-07-23, AB#0004). Status: **ratificado pela PO em 2026-07-23**. A Tag e as Cláusulas particulares vêm embutidas no **objeto da modalidade** mantido pela OnPoint, obtido **por modalidade**, e são importadas **no mesmo ciclo** da importação de catálogo (RN-034), logo após o upsert das Modalidades Importadas. Este passo herda a resiliência (RN-038) e a preservação do catálogo (RN-039). Termos novos propostos ao glossário nesta jornada: **Tag** e **Cláusula particular**. Cadência do ciclo: ver [OPEN-10](../open-decisions.md).

## RN-047 — Importação da Tag da Modalidade

**Descrição.** A Tag é o documento estruturado que descreve os campos e termos do objeto de uma Modalidade Importada — o desenho do formulário que o corretor preenche ao cotar aquela modalidade. É mantida pela OnPoint, por Seguradora e por modalidade, e trazida embutida no objeto da modalidade. Cada Modalidade Importada tem no máximo uma Tag (1:1). A importação obtém a Tag junto com o objeto da modalidade e mantém a cópia local sincronizada — o produto não desenha esses campos à mão.

**Pré-condições.** Ciclo de importação de catálogo em execução (RN-034), Modalidade Importada Ativa de Seguradora Ativa e habilitada (RN-010), e consulta ao objeto daquela modalidade na OnPoint concluída com sucesso.

**Critério de aceitação.** Quando o objeto retornado traz a Tag preenchida, a Tag daquela Modalidade Importada é criada (se ainda não existir) ou atualizada (se existir), sempre ficando Ativa; reimportar a mesma modalidade não gera duplicidade — a relação é sempre 1:1. Objeto sem Tag (ausente ou em branco) não cria Tag nem sobrescreve uma existente com vazio. A importação ocorre no ciclo agendado e pode ser disparada sob demanda pelo Administrador do Sistema, no mesmo disparo da importação de catálogo.

**Casos limite.** Modalidade Importada sem Tag local que passa a receber Tag numa importação bem-sucedida → cria. Tag existente cujo objeto volta sem Tag numa resposta bem-sucedida → o conteúdo atual é preservado (não é zerado); a retirada de circulação é tratada por reconciliação (RN-049). Modalidade Importada Inativa ou Ignorada não é consultada.

## RN-048 — Importação das Cláusulas Particulares no mesmo ciclo

**Descrição.** As Cláusulas particulares são textos contratuais opcionais vinculados à modalidade, que a OnPoint entrega no **mesmo objeto da modalidade** (mesmo payload da Tag). Por isso são importadas no mesmo ciclo. A identidade de uma Cláusula particular no produto é a combinação **Modalidade Importada + identificador da cláusula na OnPoint**. Uma Modalidade Importada pode ter N Cláusulas particulares ou nenhuma.

**Pré-condições.** Mesmas da RN-047 (ciclo de catálogo, Modalidade Importada Ativa e habilitada, consulta ao objeto concluída com sucesso). A resposta traz a lista de cláusulas da modalidade, que pode estar vazia.

**Critério de aceitação.** Cada Cláusula particular retornada é criada (se nova) ou atualizada e reativada (se já existe, reencontrada pela chave Modalidade Importada + identificador da cláusula), sem gerar duplicidade ao reimportar. Cláusula que existia localmente e não veio numa resposta bem-sucedida é inativada (RN-049), nunca apagada.

**Casos limite.** Lista de cláusulas vazia numa resposta bem-sucedida → todas as Cláusulas locais daquela modalidade são inativadas (RN-049), nenhuma apagada. Identificador de cláusula repetido na mesma resposta → a primeira ocorrência vence. Cláusula sem identificador é tratada como dado inválido: não cria item órfão.

## RN-049 — Resiliência e reconciliação de Tags e Cláusulas

**Descrição.** O passo de importação de Tags e Cláusulas herda a resiliência (RN-038) e a preservação do catálogo (RN-039): a falha na consulta do objeto de uma modalidade não desativa sua Tag nem suas Cláusulas e não interrompe as demais modalidades ou Seguradoras; nada é apagado — o que sai de circulação é inativado e pode ser reativado por uma importação futura.

**Pré-condições.** Execução da importação de catálogo (RN-034).

**Critério de aceitação.** Erro, tempo excedido ou resposta com falha na consulta do objeto de uma modalidade é registrado como falha daquela modalidade (Seguradora, identificador da modalidade, status e mensagens), sem desativar sua Tag/Cláusulas e sem afetar as demais. Após uma consulta bem-sucedida, a Tag e as Cláusulas que existiam localmente e não vieram na resposta — ou cuja Modalidade Importada não está mais Ativa — são inativadas (nunca apagadas); reaparecendo numa importação futura, são reativadas. Cada execução é auditada no sumário da importação: resultado global e a lista de falhas por recurso.

**Casos limite.** Todas as consultas de objeto falham → nada é inativado, a cópia local permanece intacta. Resposta em formato inesperado é tratada como falha, nunca como objeto vazio — não inativa nada por engano. Modalidade Importada que ficou Inativa por reconciliação de catálogo (RN-038) tem sua Tag e Cláusulas inativadas no mesmo ciclo.
